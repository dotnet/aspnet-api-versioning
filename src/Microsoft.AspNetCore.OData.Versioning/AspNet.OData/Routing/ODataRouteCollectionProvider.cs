namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    sealed class ODataRouteCollectionProvider : IODataRouteCollectionProvider
    {
        private readonly ODataRouteCollection items = new ODataRouteCollection();

        public ODataRouteCollectionProvider() { }

        public ReadOnlyKeyedCollection<ApiVersion, ODataRouteMapping> Items => items;

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