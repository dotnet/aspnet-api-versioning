namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    /// <summary>
    /// Represents a mapped OData route.
    /// </summary>
    [CLSCompliant( false )]
    public class ODataRouteMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteMapping"/> class.
        /// </summary>
        /// <param name="route">The mapped <see cref="ODataRoute">OData route</see>.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the route.</param>
        /// <param name="services">The <see cref="IServiceProvider">services</see> associated with the route.</param>
        public ODataRouteMapping( ODataRoute route, ApiVersion apiVersion, IServiceProvider services )
        {
            Route = route;
            ApiVersion = apiVersion;
            Services = services;
        }

        /// <summary>
        /// Gets the mapped OData route.
        /// </summary>
        /// <value>The mapped <see cref="ODataRoute">OData route</see>.</value>
        public ODataRoute Route { get; }

        /// <summary>
        /// Gets the API version for the route.
        /// </summary>
        /// <value>The <see cref="ApiVersion">API version</see> for the route.</value>
        public ApiVersion ApiVersion { get; }

        /// <summary>
        /// Gets the services associated with the route.
        /// </summary>
        /// <value>The <see cref="IServiceProvider">services</see> associated with the route.</value>
        public IServiceProvider Services { get; }
    }
}