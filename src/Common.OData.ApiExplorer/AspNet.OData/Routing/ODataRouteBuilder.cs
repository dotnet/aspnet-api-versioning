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
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
#if WEBAPI
    using System.Web.Http.Description;
#endif
    using static Microsoft.OData.ODataUrlKeyDelimiter;
    using static ODataRouteActionType;
    using static ODataRouteTemplateGenerationKind;
    using static System.Linq.Enumerable;
    using static System.String;
    using static System.StringComparison;
#if WEBAPI
    using static System.Web.Http.Description.ApiParameterSource;
    using static Microsoft.AspNet.OData.Routing.ODataRouteConstants;
#else
    using static Microsoft.AspNet.OData.Routing.ODataRouteConstants;
    using static Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource;
#endif
#if !API_EXPLORER
    using ApiParameterDescription = Microsoft.AspNetCore.Mvc.Abstractions.ParameterDescriptor;
#endif

    sealed partial class ODataRouteBuilder
    {
        static readonly Type ODataQueryOptionsType = typeof( ODataQueryOptions );
        static readonly Type ODataActionParametersType = typeof( ODataActionParameters );
        static readonly Dictionary<Type, string> quotedTypes = new( new TypeComparer() )
        {
            [typeof( string )] = string.Empty,
            [typeof( TimeSpan )] = "duration",
            [typeof( byte[] )] = "binary",
            [typeof( Spatial.Geography )] = "geography",
            [typeof( Spatial.Geometry )] = "geometry",
        };

        internal ODataRouteBuilder( ODataRouteBuilderContext context ) => Context = context;

        internal bool IsNavigationPropertyLink { get; private set; }

        ODataRouteBuilderContext Context { get; }

        internal string Build()
        {
            var builder = new StringBuilder();

            IsNavigationPropertyLink = false;
            BuildPath( builder );
            BuildQuery( builder );

            return builder.ToString();
        }

        internal string GetRoutePrefix() =>
            IsNullOrEmpty( Context.RoutePrefix ) ? string.Empty : RemoveRouteConstraints( Context.RoutePrefix! );

        internal IReadOnlyList<string> ExpandNavigationPropertyLinkTemplate( string template )
        {
            if ( IsNullOrEmpty( template ) )
            {
#if WEBAPI
                return new string[0];
#else
                return Array.Empty<string>();
#endif
            }

            var token = Concat( "{", NavigationProperty, "}" );

            if ( template.IndexOf( token, OrdinalIgnoreCase ) < 0 )
            {
                return new[] { template };
            }

            IEdmEntityType entity;

            switch ( Context.ActionType )
            {
                case EntitySet:
                    entity = Context.EntitySet.EntityType();
                    break;
                case Singleton:
                    entity = Context.Singleton.EntityType();
                    break;
                default:
#if WEBAPI
                    return new string[0];
#else
                    return Array.Empty<string>();
#endif
            }

            var properties = entity.NavigationProperties().ToArray();
            var refLinks = new string[properties.Length];

            for ( var i = 0; i < properties.Length; i++ )
            {
                refLinks[i] = template.Replace( token, properties[i].Name, OrdinalIgnoreCase );
            }

            return refLinks;
        }

        void BuildPath( StringBuilder builder )
        {
            var segments = new List<string>();

            AppendRoutePrefix( segments );
            AppendPath( segments );

            builder.Append( Join( "/", segments ) );
        }

        void AppendRoutePrefix( IList<string> segments )
        {
            var prefix = Context.RoutePrefix;

            if ( IsNullOrEmpty( prefix ) )
            {
                return;
            }

            prefix = RemoveRouteConstraints( prefix! );
            segments.Add( prefix );
        }

        void AppendPath( IList<string> segments )
        {
            var controllerDescriptor = Context.ActionDescriptor
#if WEBAPI
                .ControllerDescriptor
#endif
                ;

            if ( Context.IsAttributeRouted )
            {
                var prefix = default( string );
                var attribute = controllerDescriptor
#if !WEBAPI
                    .ControllerTypeInfo
#endif
                    .GetCustomAttributes<ODataRoutePrefixAttribute>()
                    .FirstOrDefault();

                if ( attribute is not null )
                {
                    prefix = attribute.Prefix?.Trim( '/' );
                }
#if !WEBAPI
                else
                {
                    var nativeEndpointRouting = Context.RouteTemplateProvider is not null;

                    if ( nativeEndpointRouting )
                    {
                        prefix = controllerDescriptor.ControllerName;
                    }
                }
#endif
                AppendPathFromAttributes( segments, prefix );
            }
            else
            {
                AppendPathFromConventions( segments, controllerDescriptor.ControllerName );
            }
        }

        void AppendPathFromAttributes( IList<string> segments, string? prefix )
        {
            var template = Context.RouteTemplate?.Replace( "[action]", Context.ActionDescriptor.ActionName, OrdinalIgnoreCase );

            if ( Context.IsOperation && Context.RouteTemplateGeneration == Client && !IsNullOrEmpty( template ) )
            {
                template = FixUpArrayParameters( template!, Context.Operation! );
            }

            if ( IsNullOrEmpty( prefix ) )
            {
                if ( !IsNullOrEmpty( template ) )
                {
                    segments.Add( template! );
                }
            }
            else
            {
                if ( IsNullOrEmpty( template ) )
                {
                    segments.Add( prefix! );
                }
                else if ( template![0] == '(' )
                {
                    segments.Add( prefix + template );
                }
                else
                {
                    segments.Add( prefix! );
                    segments.Add( template );
                }
            }
        }

        void AppendPathFromConventions( IList<string> segments, string controllerName )
        {
            var builder = new StringBuilder();

            switch ( Context.ActionType )
            {
                case EntitySet:
                    builder.Append( controllerName );
                    AppendEntityKeysFromConvention( builder );
                    AppendNavigationPropertyFromConvention( builder, Context.EntitySet.EntityType() );
                    break;
                case Singleton:
                    builder.Append( controllerName );
                    AppendNavigationPropertyFromConvention( builder, Context.Singleton.EntityType() );
                    break;
                case BoundOperation:
                    builder.Append( controllerName );
                    AppendEntityKeysFromConvention( builder );
                    segments.Add( builder.ToString() );
                    builder.Clear();
                    builder.Append( Context.Options.UseQualifiedNames ? Context.Operation.ShortQualifiedName() : Context.Operation!.Name );
                    AppendParametersFromConvention( builder, Context.Operation! );
                    break;
                case UnboundOperation:
                    builder.Append( Context.Operation!.Name );
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
            // REF: http://odata.github.io/WebApi/#13-06-KeyValueBinding
            if ( Context.EntitySet == null )
            {
                return;
            }

            var entityKeys = Context.EntitySet.EntityType().Key().ToArray();

            if ( entityKeys.Length == 0 )
            {
                return;
            }

            var parameterKeys = Context.ParameterDescriptions.Where( p => p.Name.StartsWith( Key, OrdinalIgnoreCase ) ).ToArray();

            if ( entityKeys.Length != parameterKeys.Length )
            {
                return;
            }

            var useParentheses = Context.UrlKeyDelimiter == Parentheses;
            var keySeparator = ',';
            var keyAsSegment = false;

            if ( useParentheses )
            {
                builder.Append( '(' );
            }
            else
            {
                keySeparator = '/';
                keyAsSegment = true;
                builder.Append( keySeparator );
            }

            if ( entityKeys.Length == 1 )
            {
                ExpandParameterTemplate( builder, entityKeys[0].Type, Key, keyAsSegment );
            }
            else
            {
                ExpandParameterTemplate( builder, entityKeys[0].Type, parameterKeys[0].Name, keyAsSegment );

                for ( var i = 1; i < entityKeys.Length; i++ )
                {
                    builder.Append( keySeparator );
                    ExpandParameterTemplate( builder, entityKeys[i].Type, parameterKeys[i].Name, keyAsSegment );
                }
            }

            if ( useParentheses )
            {
                builder.Append( ')' );
            }
        }

        void AppendNavigationPropertyFromConvention( StringBuilder builder, IEdmEntityType entityType )
        {
            var actionName = Context.ActionDescriptor.ActionName;
#if API_EXPLORER
            var navigationProperties = entityType.NavigationProperties().ToArray();

            IsNavigationPropertyLink = TryAppendNavigationPropertyLink( builder, actionName, navigationProperties );
#else
            IsNavigationPropertyLink = TryAppendNavigationPropertyLink( builder, actionName );
#endif

            if ( !IsNavigationPropertyLink )
            {
#if !API_EXPLORER
                var navigationProperties = entityType.NavigationProperties().ToArray();
#endif
                TryAppendNavigationProperty( builder, actionName, navigationProperties );
            }
        }

        void AppendParametersFromConvention( StringBuilder builder, IEdmOperation operation )
        {
            if ( !operation.IsFunction() )
            {
                return;
            }

            using var parameters = operation.Parameters.Where( p => p.Name != "bindingParameter" ).GetEnumerator();

            if ( !parameters.MoveNext() )
            {
                return;
            }

            var actionParameters = Context.ParameterDescriptions.ToDictionary( p => p.Name, StringComparer.OrdinalIgnoreCase );
            var parameter = parameters.Current;
            var name = parameter.Name;
            var routeParameterName = GetRouteParameterName( actionParameters, name );

            builder.Append( '(' );
            builder.Append( name );
            builder.Append( '=' );

            ExpandParameterTemplate( builder, parameter, routeParameterName );

            while ( parameters.MoveNext() )
            {
                parameter = parameters.Current;
                name = parameter.Name;
                routeParameterName = GetRouteParameterName( actionParameters, name );
                builder.Append( ',' );
                builder.Append( name );
                builder.Append( '=' );

                ExpandParameterTemplate( builder, parameter, routeParameterName );
            }

            builder.Append( ')' );
        }

        void ExpandParameterTemplate( StringBuilder template, IEdmOperationParameter parameter, string name ) =>
            ExpandParameterTemplate( template, parameter.Type, name, keyAsSegment: false );

        void ExpandParameterTemplate( StringBuilder template, IEdmTypeReference typeReference, string name, bool keyAsSegment )
        {
            var typeDef = typeReference.Definition;
            var offset = template.Length;

            template.Append( '{' );
            template.Append( name );
            template.Append( '}' );

            if ( Context.RouteTemplateGeneration == Server || keyAsSegment )
            {
                return;
            }

            switch ( typeDef.TypeKind )
            {
                case EdmTypeKind.Collection:
                    template.Insert( offset, '[' );
                    template.Append( ']' );
                    break;
                case EdmTypeKind.Enum:
                    var fullName = typeReference.FullName();

                    if ( !Context.AllowUnqualifiedEnum )
                    {
                        template.Insert( offset, fullName );
                        offset += fullName.Length;
                    }

                    template.Insert( offset, '\'' );
                    template.Append( '\'' );
                    break;
                default:
                    var type = typeDef.GetClrType( Context.EdmModel )!;

                    if ( quotedTypes.TryGetValue( type, out var prefix ) )
                    {
                        template.Insert( offset, prefix );
                        offset += prefix.Length;
                        template.Insert( offset, '\'' );
                        template.Append( '\'' );
                    }

                    break;
            }
        }

        string FixUpArrayParameters( string template, IEdmOperation operation )
        {
            if ( !operation.IsFunction() )
            {
                return template;
            }

            static int IndexOfToken( StringBuilder builder, string token )
            {
                var index = -1;

                for ( var i = 0; i < builder.Length; i++ )
                {
                    if ( builder[i] != '{' )
                    {
                        continue;
                    }

                    index = i;
                    ++i;

                    var matched = true;

                    for ( var j = 0; j < token.Length; i++, j++ )
                    {
                        if ( builder[i] != token[j] )
                        {
                            matched = false;
                            break;
                        }
                    }

                    if ( matched )
                    {
                        break;
                    }

                    while ( builder[i] != '}' )
                    {
                        ++i;
                    }
                }

                return index;
            }

            static void InsertBrackets( StringBuilder builder, string token )
            {
                var index = IndexOfToken( builder, token );

                if ( index >= 0 )
                {
                    builder.Insert( index, '[' ).Insert( index + token.Length + 3, ']' );
                }
            }

            var collectionParameters = from param in operation.Parameters
                                       where param.Type.TypeKind() == EdmTypeKind.Collection &&
                                             param.Name != "bindingParameter"
                                       select param;

            using var parameters = collectionParameters.GetEnumerator();

            if ( !parameters.MoveNext() )
            {
                return template;
            }

            var buffer = new StringBuilder( template );
            var actionParameters = Context.ParameterDescriptions.ToDictionary( p => p.Name, StringComparer.OrdinalIgnoreCase );
            var parameter = parameters.Current;
            var name = parameter.Name;
            var routeParameterName = GetRouteParameterName( actionParameters, name );

            InsertBrackets( buffer, routeParameterName );

            while ( parameters.MoveNext() )
            {
                parameter = parameters.Current;
                name = parameter.Name;
                routeParameterName = GetRouteParameterName( actionParameters, name );

                InsertBrackets( buffer, routeParameterName );
            }

            return buffer.ToString();
        }

        void BuildQuery( StringBuilder builder )
        {
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
            var queryParameters = new List<ApiParameterDescription>();
            var keys = ( Context.EntitySet?.EntityType().Key() ?? Empty<IEdmStructuralProperty>() ).ToArray();
            var operation = Context.Operation;

            for ( var i = 0; i < parameterDescriptions.Count; i++ )
            {
                var parameter = parameterDescriptions[i];
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

                var parameterType = parameter
#if API_EXPLORER
                    .ParameterDescriptor?
#endif
                    .ParameterType;

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

        bool TryAppendNavigationProperty( StringBuilder builder, string name, IReadOnlyList<IEdmNavigationProperty> navigationProperties )
        {
            if ( navigationProperties.Count == 0 )
            {
                return false;
            }

            // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/PropertyRoutingConvention.cs
            const string NavigationProperty = @"(?:Get|(?:Post|Put|Delete|Patch)To)(\w+)";
            const string NavigationPropertyFromDeclaringType = NavigationProperty + @"From(\w+)";
            var match = Regex.Match( name, NavigationPropertyFromDeclaringType, RegexOptions.Singleline );
            string propertyName;

            if ( match.Success )
            {
                propertyName = match.Groups[2].Value;
            }
            else
            {
                match = Regex.Match( name, NavigationProperty, RegexOptions.Singleline );

                if ( match.Success )
                {
                    propertyName = match.Groups[1].Value;
                }
                else
                {
                    return false;
                }
            }

            for ( var i = 0; i < navigationProperties.Count; i++ )
            {
                var navigationProperty = navigationProperties[i];

                if ( !navigationProperty.Name.Equals( propertyName, OrdinalIgnoreCase ) )
                {
                    continue;
                }

                builder.Append( '/' );

                if ( Context.Options.UseQualifiedNames )
                {
                    builder.Append( navigationProperty.Type.ShortQualifiedName() );
                }
                else
                {
                    builder.Append( propertyName );
                }

                return true;
            }

            return false;
        }

#if API_EXPLORER
        bool TryAppendNavigationPropertyLink( StringBuilder builder, string name, IReadOnlyList<IEdmNavigationProperty> navigationProperties )
#else
        bool TryAppendNavigationPropertyLink( StringBuilder builder, string name )
#endif
        {
            // REF: https://github.com/OData/WebApi/blob/master/src/Microsoft.AspNet.OData.Shared/Routing/Conventions/RefRoutingConvention.cs
            const int Link = 1;
            const int LinkTo = 2;
            const int LinkFrom = 3;
            const string NavigationPropertyLink = "(?:Create|Delete|Get)Ref";
            const string NavigationPropertyLinkTo = NavigationPropertyLink + @"To(\w+)";
            const string NavigationPropertyLinkFrom = NavigationPropertyLinkTo + @"From(\w+)";
            var i = 0;
            var patterns = new[] { NavigationPropertyLinkFrom, NavigationPropertyLinkTo, NavigationPropertyLink };
            var match = Regex.Match( name, patterns[i], RegexOptions.Singleline );

            while ( !match.Success && ++i < patterns.Length )
            {
                match = Regex.Match( name, patterns[i], RegexOptions.Singleline );
            }

            if ( !match.Success )
            {
                return false;
            }

            var convention = match.Groups.Count;
            var propertyName = match.Groups[1].Value;

            builder.Append( '/' );

            switch ( convention )
            {
                case Link:
                    builder.Append( '{' ).Append( NavigationProperty ).Append( '}' );
#if API_EXPLORER
                    RemoveNavigationPropertyParameter();
#endif
                    break;
                case LinkTo:
                case LinkFrom:
                    builder.Append( propertyName );
                    RemoveNavigationPropertyParameter();
                    break;
            }

            builder.Append( "/$ref" );

#if API_EXPLORER
            if ( name.StartsWith( "DeleteRef", Ordinal ) && !IsNullOrEmpty( propertyName ) )
            {
                var property = navigationProperties.First( p => p.Name.Equals( propertyName, OrdinalIgnoreCase ) );

                if ( property.TargetMultiplicity() == EdmMultiplicity.Many )
                {
                    AddOrReplaceRefIdQueryParameter();
                }
            }
            else if ( name.StartsWith( "CreateRef", Ordinal ) )
            {
                AddOrReplaceIdBodyParameter();
            }
#endif
            return true;
        }

        void RemoveNavigationPropertyParameter()
        {
            var parameters = Context.ParameterDescriptions;

            for ( var i = 0; i < parameters.Count; i++ )
            {
                if ( parameters[i].Name.Equals( NavigationProperty, OrdinalIgnoreCase ) )
                {
                    parameters.RemoveAt( i );
                    break;
                }
            }
        }

        static string GetRouteParameterName( IReadOnlyDictionary<string, ApiParameterDescription> actionParameters, string name )
        {
            if ( !actionParameters.TryGetValue( name, out var parameter ) )
            {
                return name;
            }
#if WEBAPI
            return parameter.ParameterDescriptor.ParameterName;
#elif API_EXPLORER
            return parameter.ParameterDescriptor.Name;
#else
            return parameter.Name;
#endif
        }

        static bool IsBuiltInParameter( Type parameterType ) =>
            ODataQueryOptionsType.IsAssignableFrom( parameterType ) || ODataActionParametersType.IsAssignableFrom( parameterType );

        static bool IsKey( IReadOnlyList<IEdmStructuralProperty> keys, ApiParameterDescription parameter )
        {
            foreach ( var key in keys )
            {
                if ( key.Name.Equals( parameter.Name, OrdinalIgnoreCase ) )
                {
                    return true;
                }
            }

            return parameter.Name.StartsWith( Key, OrdinalIgnoreCase );
        }

        static bool IsFunctionParameter( IEdmOperation? operation, ApiParameterDescription parameter )
        {
            if ( operation == null || !operation.IsFunction() )
            {
                return false;
            }

            var name = parameter.Name;

            return operation.Parameters.Any( p => p.Name.Equals( name, OrdinalIgnoreCase ) );
        }

        sealed class TypeComparer : IEqualityComparer<Type>
        {
            public bool Equals( Type? x, Type? y ) => x != null && x.IsAssignableFrom( y );

            public int GetHashCode( Type obj )
            {
                if ( obj is null )
                {
                    return 0;
                }

                if ( obj.BaseType == null || obj.BaseType.Equals( typeof( ValueType ) ) || obj.BaseType.Equals( typeof( Array ) ) )
                {
                    return obj.GetHashCode();
                }

                var baseType = typeof( object );

                while ( obj.BaseType != null && !obj.BaseType.Equals( baseType ) )
                {
                    obj = obj.BaseType;
                }

                return obj.GetHashCode();
            }
        }
    }
}