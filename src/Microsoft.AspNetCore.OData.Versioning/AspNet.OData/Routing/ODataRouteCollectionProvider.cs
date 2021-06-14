namespace Microsoft.AspNet.OData.Routing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an object that manages the collection of registered OData routes.
    /// </summary>
    [CLSCompliant( false )]
    public sealed class ODataRouteCollectionProvider : IODataRouteCollectionProvider
    {
        readonly ODataRouteCollection items = new();

        /// <inheritdoc />
        public IODataRouteCollection Items => items;

        /// <inheritdoc />
        public void Add( ODataRouteMapping item ) => items.Add( item ?? throw new ArgumentNullException( nameof( item ) ) );

        sealed class ODataRouteCollection : IODataRouteCollection
        {
            readonly List<ODataRouteMapping> items = new();

            public ODataRouteMapping this[int index] => items[index];

            public int Count => items.Count;

            public bool Contains( ODataRouteMapping item ) => items.Contains( item );

            public void CopyTo( ODataRouteMapping[] array, int index ) => items.CopyTo( array, index );

            public IEnumerator<ODataRouteMapping> GetEnumerator() => items.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            internal void Add( ODataRouteMapping item ) => items.Add( item );
        }
    }
}