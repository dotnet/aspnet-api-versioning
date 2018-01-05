namespace Microsoft.Web.Http.Description
{
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Web.Http.Description;
    using System.Web.OData;
    using System.Web.OData.Query;
    using System.Web.OData.Routing;
    using static Microsoft.OData.ODataUrlKeyDelimiter;
    using static ODataRouteActionType;
    using static System.Linq.Enumerable;
    using static System.String;
    using static System.StringComparison;
    using static System.Web.Http.Description.ApiParameterSource;

    sealed class ODataRouteBuilder
    {
        static readonly Type ODataQueryOptionsType = typeof( ODataQueryOptions );
        static readonly Type ODataActionParametersType = typeof( ODataActionParameters );
        static readonly Type GeographyType = typeof( Spatial.Geography );
        static readonly Type GeometryType = typeof( Spatial.Geometry );
        static readonly Dictionary<Type, string> quotedTypes = new Dictionary<Type, string>()
        {
            [typeof( string )] = string.Empty,
            [typeof( TimeSpan )] = "duration",
            [typeof( byte[] )] = "binary",
        };

        internal ODataRouteBuilder( ODataRouteBuilderContext context )
        {
            Contract.Requires( context != null );
            Context = context;
        }

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

            if ( !IsNullOrEmpty( prefix ) )
            {
                segments.Add( prefix );
            }
        }

        void AppendEntitySetOrOperation( IList<string> segments )
        {
            Contract.Requires( segments != null );

            var controllerDescriptor = Context.ActionDescriptor.ControllerDescriptor;

            if ( Context.IsAttributeRouted )
            {
                var prefix = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>().FirstOrDefault()?.Prefix?.Trim( '/' );
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
                    builder.Append( controllerDescriptor.ControllerName );
                    AppendEntityKeysFromConvention( builder );
                    break;
                case BoundOperation:
                    builder.Append( controllerDescriptor.ControllerName );
                    AppendEntityKeysFromConvention( builder );
                    segments.Add( builder.ToString() );
                    builder.Clear();
                    builder.Append( Context.Options.UseQualifiedOperationNames ? Context.Operation.ShortQualifiedName() : Context.Operation.Name );
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

            builder.Append( '(' );

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

            builder.Append( ')' );
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
                var routeParameterName = actionParameters[name].ParameterDescriptor.ParameterName;

                builder.Append( '(' );
                builder.Append( name );
                builder.Append( '=' );
                ExpandParameterTemplate( builder, parameter, routeParameterName );

                while ( parameters.MoveNext() )
                {
                    parameter = parameters.Current;
                    name = parameter.Name;
                    routeParameterName = actionParameters[name].ParameterDescriptor.ParameterName;
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

            template.Append( "{" );
            template.Append( name );
            template.Append( "}" );

            if ( typeDef.TypeKind == EdmTypeKind.Enum )
            {
                template.Insert( 0, '\'' );

                if ( !Context.AllowUnqualifiedEnum )
                {
                    template.Insert( 0, typeReference.FullName() );
                }

                template.Append( '\'' );
                return;
            }

            var type = typeDef.GetClrType( Context.AssembliesResolver );

            if ( quotedTypes.TryGetValue( type, out var prefix ) )
            {
                template.Insert( 0, '\'' );
                template.Insert( 0, prefix );
                template.Append( '\'' );
            }
            else if ( GeographyType.IsAssignableFrom( type ) )
            {
                template.Insert( 0, "geography'" );
                template.Append( '\'' );
            }
            else if ( GeometryType.IsAssignableFrom( type ) )
            {
                template.Insert( 0, "geometry'" );
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

        IReadOnlyList<ApiParameterDescription> GetQueryParameters( IReadOnlyList<ApiParameterDescription> parameterDescriptions )
        {
            Contract.Requires( parameterDescriptions != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiParameterDescription>>() != null );

            var queryParameters = new List<ApiParameterDescription>();
            var keys = ( Context.EntitySet?.EntityType().Key() ?? Empty<IEdmStructuralProperty>() ).ToArray();
            var operation = Context.Operation;

            foreach ( var parameter in parameterDescriptions )
            {
                if ( parameter.Source != FromUri )
                {
                    continue;
                }

                var parameterType = parameter.ParameterDescriptor?.ParameterType;

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

        bool IsBuiltInParameter( Type parameterType ) => ODataQueryOptionsType.IsAssignableFrom( parameterType ) || ODataActionParametersType.IsAssignableFrom( parameterType );

        bool IsKey( IReadOnlyList<IEdmStructuralProperty> keys, ApiParameterDescription parameter )
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

        bool IsFunctionParameter( IEdmOperation operation, ApiParameterDescription parameter )
        {
            Contract.Requires( parameter != null );

            if ( operation == null || !operation.IsFunction() )
            {
                return false;
            }

            var name = parameter.Name;

            return operation.Parameters.Any( p => p.Name.Equals( name, OrdinalIgnoreCase ) );
        }
    }
}