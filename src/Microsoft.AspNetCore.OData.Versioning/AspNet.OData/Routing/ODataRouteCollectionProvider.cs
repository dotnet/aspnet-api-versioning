namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections;
    using System.Collections.Generic;

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
        public IODataRouteCollection Items => items;

        /// <inheritdoc />
        public void Add( ODataRouteMapping item ) => items.Add( item ?? throw new ArgumentNullException( nameof( item ) ) );

        sealed class ODataRouteCollection : IODataRouteCollection
        {
            private readonly List<ODataRouteMapping> items = new List<ODataRouteMapping>();
            private readonly Dictionary<ApiVersion, List<ODataRouteMapping>> dictionary = new Dictionary<ApiVersion, List<ODataRouteMapping>>();

            public IReadOnlyList<ODataRouteMapping> this[ApiVersion key] => dictionary[key];

            public ODataRouteMapping this[int index] => items[index];

            public int Count => items.Count;

            public bool Contains( ODataRouteMapping item ) => items.Contains( item );

            public bool ContainsKey( ApiVersion key ) => dictionary.ContainsKey( key );

            public void CopyTo( ODataRouteMapping[] array, int index ) => items.CopyTo( array, index );

            public IEnumerator<ODataRouteMapping> GetEnumerator() => items.GetEnumerator();

            public int IndexOf( ODataRouteMapping item ) => items.IndexOf( item );

            public bool TryGetValue( ApiVersion key, out IReadOnlyList<ODataRouteMapping>? value )
            {
                if ( dictionary.TryGetValue( key, out var list ) )
                {
                    value = list;
                    return true;
                }

                value = default;
                return false;
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            internal void Add( ODataRouteMapping item )
            {
                var key = item.ApiVersion;

                if ( dictionary.TryGetValue( key, out var list ) )
                {
                    list.Add( item );
                }
                else
                {
                    dictionary.Add( key, new List<ODataRouteMapping>() { item } );
                }

                items.Add( item );
            }
        }
    }
}