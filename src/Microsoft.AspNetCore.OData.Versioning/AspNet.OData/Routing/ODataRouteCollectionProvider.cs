namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents an object that manages the collection of registered OData routes.
    /// </summary>
    [CLSCompliant( false )]
    public sealed class ODataRouteCollectionProvider : IODataRouteCollectionProvider
    {
        private readonly ODataRouteCollection items = new ODataRouteCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRouteCollectionProvider"/> class.
        /// </summary>
        public ODataRouteCollectionProvider() { }

        /// <inheritdoc />
        public ReadOnlyKeyedCollection<ApiVersion, ODataRouteMapping> Items => items;

        /// <inheritdoc />
        public void Add( ODataRouteMapping item )
        {
            Arg.NotNull( item, nameof( item ) );
            items.Add( item );
        }

        sealed class ODataRouteCollection : ReadOnlyKeyedCollection<ApiVersion, ODataRouteMapping>
        {
            private readonly Dictionary<ApiVersion, ODataRouteMapping> dictionary = new Dictionary<ApiVersion, ODataRouteMapping>();

            internal ODataRouteCollection() { }

            protected override IReadOnlyDictionary<ApiVersion, ODataRouteMapping> Dictionary => dictionary;

            protected override ApiVersion GetKeyForItem( ODataRouteMapping item ) => item.ApiVersion;

            internal void Add( ODataRouteMapping item )
            {
                Contract.Requires( item != null );

                dictionary.Add( GetKeyForItem( item ), item );
                Items.Add( item );
            }
        }
    }
}