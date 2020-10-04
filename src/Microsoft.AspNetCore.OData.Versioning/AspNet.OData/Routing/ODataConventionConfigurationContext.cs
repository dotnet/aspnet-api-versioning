namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.OData.Edm;
    using System;
    using System.Collections.Generic;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    public partial class ODataConventionConfigurationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionConfigurationContext"/> class.
        /// </summary>
        /// <param name="routeName">The current route name.</param>
        /// <param name="edmModel">The current <see cref="IEdmModel">EDM model</see>.</param>
        /// <param name="apiVersion">The current <see cref="ApiVersion">API version</see>.</param>
        /// <param name="routingConventions">The initial <see cref="IList{T}">list</see> of <see cref="IODataRoutingConvention">routing conventions</see>.</param>
        [CLSCompliant( false )]
        [Obsolete( "This constructor will be removed in the next major version. Use the constructor with IServiceProvider instead." )]
        public ODataConventionConfigurationContext(
            string routeName,
            IEdmModel edmModel,
            ApiVersion apiVersion,
            IList<IODataRoutingConvention> routingConventions )
            : this( routeName, edmModel, apiVersion, routingConventions, No.ServiceProvider ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionConfigurationContext"/> class.
        /// </summary>
        /// <param name="routeName">The current route name.</param>
        /// <param name="edmModel">The current <see cref="IEdmModel">EDM model</see>.</param>
        /// <param name="apiVersion">The current <see cref="ApiVersion">API version</see>.</param>
        /// <param name="routingConventions">The initial <see cref="IList{T}">list</see> of <see cref="IODataRoutingConvention">routing conventions</see>.</param>
        /// <param name="serviceProvider">The associated <see cref="IServiceProvider">serviceProvider</see>.</param>
        [CLSCompliant( false )]
        public ODataConventionConfigurationContext(
            string routeName,
            IEdmModel edmModel,
            ApiVersion apiVersion,
            IList<IODataRoutingConvention> routingConventions,
            IServiceProvider serviceProvider )
        {
            RouteName = routeName;
            EdmModel = edmModel;
            ApiVersion = apiVersion;
            RoutingConventions = routingConventions;
            ServiceProvider = serviceProvider;
        }
    }
}