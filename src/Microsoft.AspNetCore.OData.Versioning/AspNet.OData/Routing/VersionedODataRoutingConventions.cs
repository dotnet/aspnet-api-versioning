namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Routing.Conventions;
    using System;
    using System.Collections.Generic;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public static partial class VersionedODataRoutingConventions
    {
        /// <summary>
        /// Creates a mutable list of the default OData routing conventions with attribute routing enabled.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A mutable list of the default OData routing conventions.</returns>
        /// <remarks>Use this version for Endpoint Routing.</remarks>
        public static IList<IODataRoutingConvention> CreateDefaultWithAttributeRouting( string routeName, IServiceProvider serviceProvider ) =>
            EnsureConventions( CreateDefault(), new VersionedAttributeRoutingConvention( routeName, serviceProvider ) );
    }
}