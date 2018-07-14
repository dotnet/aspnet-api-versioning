namespace Microsoft.AspNet.OData
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents a read-only, keyed collection.
    /// </summary>
    /// <typeparam name="TKey">The type of key.</typeparam>
    /// <typeparam name="TItem">The type of item.</typeparam>
    public abstract class ReadOnlyKeyedCollection<TKey, TItem> : ReadOnlyCollection<TItem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyKeyedCollection{TKey, TItem}"/> class.
        /// </summary>
        protected ReadOnlyKeyedCollection() : this( new List<TItem>(), EqualityComparer<TKey>.Default ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyKeyedCollection{TKey, TItem}"/> class.
        /// </summary>
        /// <param name="comparer">The <see cref="IComparer{T}">comparer</see> used to compare item <typeparamref name="TKey">keys</typeparamref>.</param>
        protected ReadOnlyKeyedCollection( IEqualityComparer<TKey> comparer ) : this( new List<TItem>(), comparer ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyKeyedCollection{TKey, TItem}"/> class.
        /// </summary>
        /// <param name="items">The <see cref="IList{T}">list</see> of <typeparamref name="TItem">items</typeparamref> in the collection.</param>
        /// <param name="comparer">The <see cref="IComparer{T}">comparer</see> used to compare item <typeparamref name="TKey">keys</typeparamref>.</param>
        protected ReadOnlyKeyedCollection( IList<TItem> items, IEqualityComparer<TKey> comparer ) : base( items )
        {
            Comparer = comparer;
        }

        /// <summary>
        /// Gets the item in the collection with the specified key.
        /// </summary>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>The <typeparamref name="TItem">item</typeparamref> with the specified <paramref name="key"/>.</returns>
        public TItem this[TKey key] => Dictionary[key];

        /// <summary>
        /// Gets the comparer used for item keys.
        /// </summary>
        /// <value>The <see cref="IEqualityComparer{T}">comparer</see> used for item <typeparamref name="TKey">keys</typeparamref>.</value>
        public IEqualityComparer<TKey> Comparer { get; }

        /// <summary>
        /// Gets the key/value pairs mapping keys and items.
        /// </summary>
        /// <value>A <see cref="IReadOnlyDictionary{TKey, TValue}">read-only collection</see> of key/item pairs.</value>
        protected abstract IReadOnlyDictionary<TKey, TItem> Dictionary { get; }

        /// <summary>
        /// Gets a value indicating whether the collection contains the specified key.
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey">key</typeparamref> to evaluate.</param>
        /// <returns>True if the collection contains the key; otherwise, false.</returns>
        public bool Contains( TKey key ) => Dictionary.ContainsKey( key );

        /// <summary>
        /// Attempts to get an item from the collection with the specified key.
        /// </summary>
        /// <param name="key">The <typeparamref name="TKey">key</typeparamref> to evaluate.</param>
        /// <param name="item">The retrieved <typeparamref name="TItem">item</typeparamref> or a default value.</param>
        /// <returns>True if the item was successfully retrieved; otherwise, false.</returns>
        public bool TryGetValue( TKey key, out TItem item ) => Dictionary.TryGetValue( key, out item );

        /// <summary>
        /// Gets the key for the specified item.
        /// </summary>
        /// <param name="item">The <typeparamref name="TItem">item</typeparamref> to get a key for.</param>
        /// <returns>The <typeparamref name="TKey">key</typeparamref> for the <paramref name="item"/>.</returns>
        protected abstract TKey GetKeyForItem( TItem item );
    }
}