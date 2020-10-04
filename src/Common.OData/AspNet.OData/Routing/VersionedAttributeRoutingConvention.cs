namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Template;
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
#endif
    using Microsoft.OData;
    using Microsoft.OData.UriParser;
#if WEBAPI
    using Microsoft.Web.Http;
#endif
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using static System.StringComparison;
#if WEBAPI
    using ControllerActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
#endif

    /// <summary>
    /// Represents an OData attribute routing convention with additional support for API versioning.
    /// </summary>
    public partial class VersionedAttributeRoutingConvention
    {
        readonly ConcurrentDictionary<ApiVersion, IReadOnlyDictionary<ODataPathTemplate, ControllerActionDescriptor>> attributeMappingsPerApiVersion =
            new ConcurrentDictionary<ApiVersion, IReadOnlyDictionary<ODataPathTemplate, ControllerActionDescriptor>>();

        /// <summary>
        /// Gets the name of the route associated the routing convention.
        /// </summary>
        /// <value>The name of the associated route.</value>
        public string RouteName { get; }

        /// <summary>
        /// Gets the <see cref="IODataPathTemplateHandler"/> to be used for parsing the route templates.
        /// </summary>
        /// <value>The <see cref="IODataPathTemplateHandler"/> to be used for parsing the route templates.</value>
        public IODataPathTemplateHandler ODataPathTemplateHandler { get; }

        static IEnumerable<string> GetODataRoutePrefixes( IEnumerable<ODataRoutePrefixAttribute> prefixAttributes, string controllerName )
        {
            using var prefixAttribute = prefixAttributes.GetEnumerator();

            if ( !prefixAttribute.MoveNext() )
            {
                yield return string.Empty;
                yield break;
            }

            do
            {
                yield return GetODataRoutePrefix( prefixAttribute.Current, controllerName );
            }
            while ( prefixAttribute.MoveNext() );
        }

        static string GetODataRoutePrefix( ODataRoutePrefixAttribute prefixAttribute, string controllerName )
        {
            var prefix = prefixAttribute.Prefix;

            if ( prefix != null && prefix.StartsWith( "/", Ordinal ) )
            {
                throw new InvalidOperationException( SR.RoutePrefixStartsWithSlash.FormatDefault( prefix, controllerName ) );
            }

            if ( prefix != null && prefix.EndsWith( "/", Ordinal ) )
            {
                prefix = prefix.TrimEnd( '/' );
            }

            return prefix ?? string.Empty;
        }

        static bool IsODataRouteParameter( KeyValuePair<string, object> routeDatum )
        {
            // REF: https://github.com/OData/WebApi/blob/feature/netcore/src/Microsoft.AspNet.OData.Shared/Routing/ODataParameterValue.cs
            const string ParameterValuePrefix = "DF908045-6922-46A0-82F2-2F6E7F43D1B1_";
            const string ODataParameterValue = nameof( ODataParameterValue );

            return routeDatum.Key.StartsWith( ParameterValuePrefix, Ordinal ) && routeDatum.Value?.GetType().Name == ODataParameterValue;
        }

        ODataPathTemplate? GetODataPathTemplate( string prefix, string pathTemplate, IServiceProvider serviceProvider )
        {
            if ( prefix != null && !pathTemplate.StartsWith( "/", Ordinal ) )
            {
                if ( string.IsNullOrEmpty( pathTemplate ) )
                {
                    pathTemplate = prefix;
                }
                else if ( pathTemplate.StartsWith( "(", Ordinal ) )
                {
                    pathTemplate = prefix + pathTemplate;
                }
                else
                {
                    pathTemplate = prefix + "/" + pathTemplate;
                }
            }

            if ( pathTemplate.StartsWith( "/", Ordinal ) )
            {
                pathTemplate = pathTemplate.Substring( 1 );
            }

            return ODataPathTemplateHandler.SafeParseTemplate( pathTemplate, serviceProvider );
        }
    }
}