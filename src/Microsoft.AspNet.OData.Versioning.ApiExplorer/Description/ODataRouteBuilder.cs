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
    using static System.Linq.Enumerable;
    using static System.String;
    using static System.StringComparison;
    using static System.Web.Http.Description.ApiParameterSource;

    sealed class ODataRouteBuilder
    {
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
            var prefix = Context.Route.RoutePrefix?.Trim( '/' );

            if ( !IsNullOrEmpty( prefix ) )
            {
                segments.Add( prefix );
            }

            var path = GetEntitySetSegment() + GetEntityKeySegment();

            segments.Add( path );
            builder.Append( Join( "/", segments ) );
        }

        void BuildQuery( StringBuilder builder )
        {
            Contract.Requires( builder != null );

            var queryParameters = FilterQueryParameters( Context.ParameterDescriptions );

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

        string GetEntitySetSegment()
        {
            var controllerDescriptor = Context.ActionDescriptor.ControllerDescriptor;
            var prefix = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>().FirstOrDefault()?.Prefix?.Trim( '/' );
            return IsNullOrEmpty( prefix ) ? controllerDescriptor.ControllerName : prefix;
        }

        string GetEntityKeySegment()
        {
            var template = Context.ActionDescriptor.GetCustomAttributes<ODataRouteAttribute>().FirstOrDefault()?.PathTemplate;

            if ( !IsNullOrEmpty( template ) )
            {
                return template;
            }

            var keys = Context.EntityKeys.Where( key => Context.ParameterDescriptions.Any( p => key.Name.Equals( p.Name, OrdinalIgnoreCase ) ) );
            var convention = new StringBuilder();

            using ( var iterator = keys.GetEnumerator() )
            {
                if ( iterator.MoveNext() )
                {
                    convention.Append( '(' );

                    var key = iterator.Current;

                    if ( iterator.MoveNext() )
                    {
                        convention.Append( key.Name );
                        convention.Append( '=' );
                        ExpandParameterTemplate( convention, key );

                        while ( iterator.MoveNext() )
                        {
                            convention.Append( ',' );
                            convention.Append( key.Name );
                            convention.Append( '=' );
                            ExpandParameterTemplate( convention, key );
                        }
                    }
                    else
                    {
                        ExpandParameterTemplate( convention, key );
                    }

                    convention.Append( ')' );
                }
                else
                {
                    TryAddEntityKeySegmentByConvention( convention );
                }
            }

            return convention.ToString();
        }

        void TryAddEntityKeySegmentByConvention( StringBuilder convention )
        {
            // REF: http://odata.github.io/WebApi/#13-06-KeyValueBinding
            var entityKeys = Context.EntityKeys.ToArray();
            var parameterKeys = Context.ParameterDescriptions.Where( p => p.Name.StartsWith( ODataRouteConstants.Key, OrdinalIgnoreCase ) ).ToArray();

            if ( entityKeys.Length == parameterKeys.Length )
            {
                return;
            }

            convention.Append( '(' );

            if ( entityKeys.Length == 1 )
            {
                ExpandParameterTemplate( convention, entityKeys[0], ODataRouteConstants.Key );
            }
            else
            {
                for ( var i = 0; i < entityKeys.Length; i++ )
                {
                    ExpandParameterTemplate( convention, entityKeys[i], parameterKeys[i].Name );
                }
            }

            convention.Append( ')' );
        }

        void ExpandParameterTemplate( StringBuilder template, IEdmStructuralProperty key ) => ExpandParameterTemplate( template, key, key?.Name );

        void ExpandParameterTemplate( StringBuilder template, IEdmStructuralProperty key, string name )
        {
            Contract.Requires( template != null );
            Contract.Requires( key != null );
            Contract.Requires( !IsNullOrEmpty( name ) );

            var typeDef = key.Type.Definition;

            template.Append( "{" );
            template.Append( name );
            template.Append( "}" );

            if ( typeDef.TypeKind == EdmTypeKind.Enum )
            {
                template.Insert( 0, '\'' );

                if ( !Context.AllowUnqualifiedEnum )
                {
                    template.Insert( 0, key.Type.FullName() );
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

        IReadOnlyList<ApiParameterDescription> FilterQueryParameters( IReadOnlyList<ApiParameterDescription> parameterDescriptions )
        {
            Contract.Requires( parameterDescriptions != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ApiParameterDescription>>() != null );

            var queryParameters = new List<ApiParameterDescription>();
            var queryOptions = typeof( ODataQueryOptions );
            var actionParameters = typeof( ODataActionParameters );

            foreach ( var parameter in parameterDescriptions )
            {
                if ( parameter.Source != FromUri )
                {
                    continue;
                }

                var parameterType = parameter.ParameterDescriptor?.ParameterType;

                if ( parameterType == null ||
                     queryOptions.IsAssignableFrom( parameterType ) ||
                     actionParameters.IsAssignableFrom( parameterType ) )
                {
                    continue;
                }

                if ( !Context.EntityKeys.Any( key => key.Name.Equals( parameter.Name, OrdinalIgnoreCase ) ) )
                {
                    queryParameters.Add( parameter );
                }
            }

            return queryParameters;
        }
    }
}