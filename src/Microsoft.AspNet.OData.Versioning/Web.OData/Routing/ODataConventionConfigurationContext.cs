namespace Microsoft.Web.OData.Routing
{
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.OData.Routing.Conventions;

    /// <summary>
    /// Represents the context used to configure OData routing conventions.
    /// </summary>
    public class ODataConventionConfigurationContext
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
            Arg.NotNull( configuration, nameof( configuration ) );
            Arg.NotNullOrEmpty( routeName, nameof( routeName ) );
            Arg.NotNull( edmModel, nameof( edmModel ) );
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Arg.NotNull( routingConventions, nameof( routingConventions ) );

            Configuration = configuration;
            RouteName = routeName;
            EdmModel = edmModel;
            ApiVersion = ApiVersion;
            RoutingConventions = routingConventions;
        }

        /// <summary>
        /// Gets the configuration associated with the routing conventions.
        /// </summary>
        /// <value>The current <see cref="HttpConfiguration">configuration</see>.</value>
        public HttpConfiguration Configuration { get; }

        /// <summary>
        /// Gets the name of the route the conventions are being created for.
        /// </summary>
        /// <value>The current route name.</value>
        public string RouteName { get; }

        /// <summary>
        /// Gets the entity data model (EDM) associcated with the routing conventions.
        /// </summary>
        /// <value>The current <see cref="IEdmModel">EDM model</see>.</value>
        public IEdmModel EdmModel { get; }

        /// <summary>
        /// Gets the API version associated with the routing conventions.
        /// </summary>
        /// <value>The current <see cref="ApiVersion">API version</see>.</value>
        public ApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets the initial list of routing conventions.
        /// </summary>
        /// <value>The initial <see cref="IList{T}">list</see> of <see cref="IODataRoutingConvention">routing conventions</see>.</value>
        public IList<IODataRoutingConvention> RoutingConventions { get; }
    }
}