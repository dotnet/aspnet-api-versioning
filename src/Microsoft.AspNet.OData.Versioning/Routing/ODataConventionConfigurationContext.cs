namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using System.Collections.Generic;
    using System.Web.Http;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial class ODataConventionConfigurationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionConfigurationContext"/> class.
        /// </summary>
        /// <param name="configuration">The current <see cref="HttpConfiguration">configuration</see>.</param>
        /// <param name="routeName">The current route name.</param>
        /// <param name="edmModel">The current <see cref="IEdmModel">EDM model</see>.</param>
        /// <param name="apiVersion">The current <see cref="ApiVersion">API version</see>.</param>
        /// <param name="routingConventions">The initial <see cref="IList{T}">list</see> of <see cref="IODataRoutingConvention">routing conventions</see>.</param>
        public ODataConventionConfigurationContext( HttpConfiguration configuration, string routeName, IEdmModel edmModel, ApiVersion apiVersion, IList<IODataRoutingConvention> routingConventions )
        {
            Configuration = configuration;
            RouteName = routeName;
            EdmModel = edmModel;
            ApiVersion = apiVersion;
            RoutingConventions = routingConventions;
        }

        /// <summary>
        /// Gets the configuration associated with the routing conventions.
        /// </summary>
        /// <value>The current <see cref="HttpConfiguration">configuration</see>.</value>
        public HttpConfiguration Configuration { get; }
    }
}