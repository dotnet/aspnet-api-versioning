namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using System;

    /// <summary>
    /// Represents a mapped OData route.
    /// </summary>
    [CLSCompliant( false )]
    public class ODataRouteMapping
    {
        readonly IServiceProvider serviceProvider;
        IServiceProvider? services;
        IEdmModelSelector? modelSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteMapping"/> class.
        /// </summary>
        /// <param name="routeName">The OData route name.</param>
        /// <param name="routePrefix">The OData route prefix.</param>
        /// <param name="services">The <see cref="IServiceProvider">services</see> associated with the route.</param>
        public ODataRouteMapping( string routeName, string? routePrefix, IServiceProvider services )
        {
            RouteName = routeName;
            RoutePrefix = routePrefix?.Trim( '/' );
            serviceProvider = services;
        }

        /// <summary>
        /// Gets the name of the mapped OData route.
        /// </summary>
        /// <value>The OData route name.</value>
        public string RouteName { get; }

        /// <summary>
        /// Gets the prefix of the mapped OData route.
        /// </summary>
        /// <value>The OData route prefix.</value>
        public string? RoutePrefix { get; }

        /// <summary>
        /// Gets the EDM model selector.
        /// </summary>
        /// <value>The associated <see cref="IEdmModelSelector">EDM model selector</see>.</value>
        public IEdmModelSelector ModelSelector => modelSelector ??= Services.GetRequiredService<IEdmModelSelector>();

        /// <summary>
        /// Gets the services associated with the route.
        /// </summary>
        /// <value>The <see cref="IServiceProvider">services</see> associated with the route.</value>
        public IServiceProvider Services =>
            services ??= serviceProvider.GetRequiredService<IPerRouteContainer>().GetODataRootContainer( RouteName );
    }
}