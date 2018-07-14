namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc;
    using System;

    /// <summary>
    /// Defines the behavior of an OData route collection provider.
    /// </summary>
    [CLSCompliant( false )]
    public interface IODataRouteCollectionProvider
    {
        /// <summary>
        /// Gets the collection of mapped OData routes.
        /// </summary>
        /// <value>A <see cref="ReadOnlyKeyedCollection{TKey, TItem}">read-only collection</see> of
        /// <see cref="ODataRouteMapping">mapped OData routes</see>.</value>
        ReadOnlyKeyedCollection<ApiVersion, ODataRouteMapping> Items { get; }

        /// <summary>
        /// Adds a mapped OData route.
        /// </summary>
        /// <param name="item">The <see cref="ODataRouteMapping">mapped OData route</see> to add.</param>
        void Add( ODataRouteMapping item );
    }
}