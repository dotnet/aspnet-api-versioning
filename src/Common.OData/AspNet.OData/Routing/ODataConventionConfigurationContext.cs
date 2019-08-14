namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Conventions;
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc;
#endif
    using Microsoft.OData.Edm;
#if WEBAPI
    using Microsoft.Web.Http;
#endif
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the context used to configure OData routing conventions.
    /// </summary>
    public partial class ODataConventionConfigurationContext
    {
        /// <summary>
        /// Gets the name of the route the conventions are being created for.
        /// </summary>
        /// <value>The current route name.</value>
        public string RouteName { get; }

        /// <summary>
        /// Gets the entity data model (EDM) associated with the routing conventions.
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
#if !WEBAPI
        [CLSCompliant( false )]
#endif
        public IList<IODataRoutingConvention> RoutingConventions { get; }
    }
}