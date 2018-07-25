namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the behavior of an OData route collection provider.
    /// </summary>
    [CLSCompliant( false )]
    public interface IODataRouteCollectionProvider
    {
        /// <summary>
        /// Gets the collection of mapped OData routes.
        /// </summary>
        /// <value>A <see cref="IODataRouteCollection">read-only collection</see> of
        /// <see cref="ApiVersion">API versions</see> mapped to a <see cref="IReadOnlyList{T}">read-only list</see>
        /// <see cref="ODataRouteMapping">mapped OData routes</see>.</value>
        IODataRouteCollection Items { get; }

        /// <summary>
        /// Adds a mapped OData route.
        /// </summary>
        /// <param name="item">The <see cref="ODataRouteMapping">mapped OData route</see> to add.</param>
        void Add( ODataRouteMapping item );
    }
}