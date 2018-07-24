namespace Microsoft.AspNet.OData.Routing
{
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
#endif
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Text;
#if WEBAPI
    using System.Web.Http.Description;
#endif
    using static Microsoft.OData.ODataUrlKeyDelimiter;
    using static ODataRouteActionType;
    using static System.Linq.Enumerable;
    using static System.String;
    using static System.StringComparison;
#if WEBAPI
    using static System.Web.Http.Description.ApiParameterSource;
#else
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;
#endif
#if !API_EXPLORER
    using ApiParameterDescription = Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor;
#endif

    sealed partial class ODataRouteBuilder
    {
        static readonly Type ODataQueryOptionsType = typeof( ODataQueryOptions );
        static readonly Type ODataActionParametersType = typeof( ODataActionParameters );
        static readonly Dictionary<Type, string> quotedTypes = new Dictionary<Type, string>( new TypeComparer() )
        {
            [typeof( string )] = string.Empty,
            [typeof( TimeSpan )] = "duration",
            [typeof( byte[] )] = "binary",
            [typeof( Spatial.Geography )] = "geography",
            [typeof( Spatial.Geometry )] = "geometry",
        };

        internal ODataRouteBuilder( ODataRouteBuilderContext context ) => Context = context;

        internal string Build()
        {
            var builder = new StringBuilder();

            BuildPath( builder );
            BuildQuery( builder );

            return builder.ToString();
        }

        ODataRouteBuilderContext Context { get; }

        void BuildPath( StringBuilder builder )
        {
            Contract.Requires( builder != null );

            var segments = new List<string>();

            AppendRoutePrefix( segments );
            AppendEntitySetOrOperation( segments );

            builder.Append( Join( "/", segments ) );
        }

        void AppendRoutePrefix( IList<string> segments )
        {
            Contract.Requires( segments != null );

            var prefix = Context.Route.RoutePrefix?.Trim( '/' );

            if ( IsNullOrEmpty( prefix ) )
            {
                return;
            }

            prefix = UpdateRoutePrefixAndRemoveApiVersionParameterIfNecessary( prefix );
            segments.Add( prefix );
        }

        void AppendEntitySetOrOperation( IList<string> segments )
        {
            Contract.Requires( segments != null );

#if WEBAPI
            var controllerDescriptor = Context.ActionDescriptor.ControllerDescriptor;
#else
            var controllerDescriptor = Context.ActionDescriptor;
#endif
            var controllerName = controllerDescriptor.ControllerName;

            if ( Context.IsAttributeRouted )
            {
#if WEBAPI
                var prefix = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>().FirstOrDefault()?.Prefix?.Trim( '/' );
#else
                var prefix = controllerDescriptor.ControllerTypeInfo.GetCustomAttributes<ODataRoutePrefixAttribute>().FirstOrDefault()?.Prefix?.Trim( '/' );
#endif
                var template = Context.RouteTemplate;

                if ( IsNullOrEmpty( prefix ) )
                {
                    segments.Add( Context.RouteTemplate );
                }
                else
                {
                    if ( IsNullOrEmpty( template ) )
                    {
                        segments.Add( prefix );
                    }
                    else if ( template[0] == '(' && Context.UrlKeyDelimiter == Parentheses )
                    {
                        segments.Add( prefix + template );
                    }
                    else
                    {
                        segments.Add( prefix );
                        segments.Add( template );
                    }
                }

                return;
            }

            var builder = new StringBuilder();

            switch ( Context.ActionType )
            {
                case EntitySet:
                    builder.Append( controllerName );
                    AppendEntityKeysFromConvention( builder );
                    break;
                case BoundOperation:
                    builder.Append( controllerName );
                    AppendEntityKeysFromConvention( builder );
                    segments.Add( builder.ToString() );
                    builder.Clear();
#if API_EXPLORER
                    builder.Append( Context.Options.UseQualifiedOperationNames ? Context.Operation.ShortQualifiedName() : Context.Operation.Name );
#else
                    builder.Append( Context.Operation.ShortQualifiedName() );
#endif
                    AppendParametersFromConvention( builder, Context.Operation );
                    break;
                case UnboundOperation:
                    builder.Append( Context.Operation.Name );
                    AppendParametersFromConvention( builder, Context.Operation );
                    break;
            }

            if ( builder.Length > 0 )
            {
                segments.Add( builder.ToString() );
            }
        }

        void AppendEntityKeysFromConvention( StringBuilder builder )
        {
            Contract.Requires( builder != null );

            // REF: http://odata.github.io/WebApi/#13-06-KeyValueBinding
            var entityKeys = ( Context.EntitySet?.EntityType().Key() ?? Empty<IEdmStructuralProperty>() ).ToArray();
            var parameterKeys = Context.ParameterDescriptions.Where( p => p.Name.StartsWith( ODataRouteConstants.Key, OrdinalIgnoreCase ) ).ToArray();

            if ( entityKeys.Length != parameterKeys.Length )
            {
                return;
            }

            var useParentheses = Context.UrlKeyDelimiter == Parentheses;

            if ( useParentheses )
            {
                builder.Append( '(' );
            }
            else
            {
                builder.Append( '/' );
            }

            if ( entityKeys.Length == 1 )
            {
                ExpandParameterTemplate( builder, entityKeys[0], ODataRouteConstants.Key );
            }
            else
            {
                for ( var i = 0; i < entityKeys.Length; i++ )
                {
                    ExpandParameterTemplate( builder, entityKeys[i], parameterKeys[i].Name );
                }
            }

            if ( useParentheses )
            {
                builder.Append( ')' );
            }
        }

        void AppendParametersFromConvention( StringBuilder builder, IEdmOperation operation )
        {
            Contract.Requires( builder != null );
            Contract.Requires( operation != null );

            if ( !operation.IsFunction() )
            {
                return;
            }

            using ( var parameters = operation.Parameters.Where( p => p.Name != "bindingParameter" ).GetEnumerator() )
            {
                if ( !parameters.MoveNext() )
                {
                    return;
                }

                var actionParameters = Context.ParameterDescriptions.ToDictionary( p => p.Name, StringComparer.OrdinalIgnoreCase );
                var parameter = parameters.Current;
                var name = parameter.Name;
#if WEBAPI
                var routeParameterName = actionParameters[name].ParameterDescriptor.ParameterName;
#elif API_EXPLORER
                var routeParameterName = actionParameters[name].ParameterDescriptor.Name;
#else
                var routeParameterName = actionParameters[name].Name;
#endif

                builder.Append( '(' );
                builder.Append( name );
                builder.Append( '=' );
                ExpandParameterTemplate( builder, parameter, routeParameterName );

                while ( parameters.MoveNext() )
                {
                    parameter = parameters.Current;
                    name = parameter.Name;
#if WEBAPI
                    routeParameterName = actionParameters[name].ParameterDescriptor.ParameterName;
#elif API_EXPLORER
                    routeParameterName = actionParameters[name].ParameterDescriptor.Name;
#else
                    routeParameterName = actionParameters[name].Name;
#endif
                    builder.Append( ',' );
                    builder.Append( name );
                    builder.Append( '=' );
                    ExpandParameterTemplate( builder, parameter, routeParameterName );
                }

                builder.Append( ')' );
            }
        }

        void ExpandParameterTemplate( StringBuilder template, IEdmStructuralProperty key ) => ExpandParameterTemplate( template, key.Type, key.Name );

        void ExpandParameterTemplate( StringBuilder template, IEdmStructuralProperty key, string name ) => ExpandParameterTemplate( template, key.Type, name );

        void ExpandParameterTemplate( StringBuilder template, IEdmOperationParameter parameter, string name ) => ExpandParameterTemplate( template, parameter.Type, name );

        void ExpandParameterTemplate( StringBuilder template, IEdmTypeReference typeReference, string name )
        {
            Contract.Requires( template != null );
            Contract.Requires( typeReference != null );
            Contract.Requires( !IsNullOrEmpty( name ) );

            var typeDef = typeReference.Definition;
            var offset = template.Length;

            template.Append( "{" );
            template.Append( name );
            template.Append( "}" );

            if ( typeDef.TypeKind == EdmTypeKind.Enum )
            {
                var fullName = typeReference.FullName();

                if ( !Context.AllowUnqualifiedEnum )
                {
                    template.Insert( offset, fullName );
                    offset += fullName.Length;
                }

                template.Insert( offset, '\'' );
                template.Append( '\'' );
                return;
            }

            var type = typeDef.GetClrType( Context.Assemblies );

            if ( quotedTypes.TryGetValue( type, out var prefix ) )
            {
                template.Insert( offset, prefix );
                offset += prefix.Length;
                template.Insert( offset, '\'' );
                template.Append( '\'' );
            }
        }

        void BuildQuery( StringBuilder builder )
        {
            Contract.Requires( builder != null );

            var queryParameters = GetQueryParameters( Context.ParameterDescriptions );

            if ( queryParameters.Count == 0 )
            {
                return;
            }

            var queryString = new StringBuilder();

            using ( var iterator = queryParameters.GetEnumerator() )
            {
                iterator.MoveNext();
                var name = iterator.Current.Name;

                queryString.Append( name );
                queryString.Append( "={" );
                queryString.Append( name );
                queryString.Append( '}' );

                while ( iterator.MoveNext() )
                {
                    name = iterator.Current.Name;
                    queryString.Append( '&' );
                    queryString.Append( name );
                    queryString.Append( "={" );
                    queryString.Append( name );
                    queryString.Append( '}' );
                }
            }

            if ( queryString.Length > 0 )
            {
                builder.Append( '?' );
                builder.Append( queryString );
            }
        }

        IList<ApiParameterDescription> GetQueryParameters( IList<ApiParameterDescription> parameterDescriptions )
        {
            Contract.Requires( parameterDescriptions != null );
            Contract.Ensures( Contract.Result<IList<ApiParameterDescription>>() != null );

            var queryParameters = new List<ApiParameterDescription>();
            var keys = ( Context.EntitySet?.EntityType().Key() ?? Empty<IEdmStructuralProperty>() ).ToArray();
            var operation = Context.Operation;

            foreach ( var parameter in parameterDescriptions )
            {
#if WEBAPI
                if ( parameter.Source != FromUri )
#elif API_EXPLORER
                if ( parameter.Source != Query )
#else
                if ( parameter.BindingInfo.BindingSource != Query )
#endif
                {
                    continue;
                }

#if API_EXPLORER
                var parameterType = parameter.ParameterDescriptor?.ParameterType;
#else
                var parameterType = parameter.ParameterType;
#endif

                if ( parameterType == null || IsBuiltInParameter( parameterType ) )
                {
                    continue;
                }

                if ( IsKey( keys, parameter ) || IsFunctionParameter( operation, parameter ) )
                {
                    continue;
                }

                queryParameters.Add( parameter );
            }

            return queryParameters;
        }

        static bool IsBuiltInParameter( Type parameterType ) => ODataQueryOptionsType.IsAssignableFrom( parameterType ) || ODataActionParametersType.IsAssignableFrom( parameterType );

        static bool IsKey( IReadOnlyList<IEdmStructuralProperty> keys, ApiParameterDescription parameter )
        {
            Contract.Requires( keys != null );
            Contract.Requires( parameter != null );

            foreach ( var key in keys )
            {
                if ( key.Name.Equals( parameter.Name, OrdinalIgnoreCase ) )
                {
                    return true;
                }
            }

            return parameter.Name.StartsWith( ODataRouteConstants.Key, OrdinalIgnoreCase );
        }

        static bool IsFunctionParameter( IEdmOperation operation, ApiParameterDescription parameter )
        {
            Contract.Requires( parameter != null );

            if ( operation == null || !operation.IsFunction() )
            {
                return false;
            }

            var name = parameter.Name;

            return operation.Parameters.Any( p => p.Name.Equals( name, OrdinalIgnoreCase ) );
        }

        sealed class TypeComparer : IEqualityComparer<Type>
        {
            public bool Equals( Type x, Type y ) => x.IsAssignableFrom( y );

            public int GetHashCode( Type obj )
            {
                if ( obj.BaseType.Equals( typeof( ValueType ) ) || obj.BaseType.Equals( typeof( Array ) ) )
                {
                    return obj.GetHashCode();
                }

                var baseType = typeof( object );

                while ( !obj.BaseType.Equals( baseType ) )
                {
                    obj = obj.BaseType;
                }

                return obj.GetHashCode();
            }
        }
    }
}