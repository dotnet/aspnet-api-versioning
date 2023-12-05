// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using System.Collections;

/// <summary>
/// Represents a collection of collated API version metadata.
/// </summary>
public class ApiVersionMetadataCollationCollection : IList<ApiVersionMetadata>, IReadOnlyList<ApiVersionMetadata>
{
    private readonly List<ApiVersionMetadata> items;
    private readonly List<string?> groups;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionMetadataCollationCollection"/> class.
    /// </summary>
    public ApiVersionMetadataCollationCollection()
    {
        items = [];
        groups = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionMetadataCollationCollection"/> class.
    /// </summary>
    /// <param name="capacity">The initial capacity of the collection.</param>
    public ApiVersionMetadataCollationCollection( int capacity )
    {
        items = new( capacity );
        groups = new( capacity );
    }

    /// <summary>
    /// Gets the item in the list at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to retrieve.</param>
    /// <returns>The item at the specified index.</returns>
    public ApiVersionMetadata this[int index] => items[index];

    ApiVersionMetadata IList<ApiVersionMetadata>.this[int index]
    {
        get => items[index];
        set => throw new NotSupportedException();
    }

    /// <inheritdoc />
    public int Count => items.Count;

#pragma warning disable IDE0079
#pragma warning disable CA1033 // Interface methods should be callable by child types
    bool ICollection<ApiVersionMetadata>.IsReadOnly => ( (ICollection<ApiVersionMetadata>) items ).IsReadOnly;
#pragma warning restore CA1033 // Interface methods should be callable by child types
#pragma warning restore IDE0079

    /// <inheritdoc />
    public void Add( ApiVersionMetadata item ) => Insert( Count, item, default );

    /// <summary>
    /// Adds an item to the collection.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <param name="groupName">The associated group name, if any.</param>
    public void Add( ApiVersionMetadata item, string? groupName ) => Insert( Count, item, groupName );

    /// <inheritdoc />
    public void Clear()
    {
        items.Clear();
        groups.Clear();
    }

    /// <inheritdoc />
    public bool Contains( ApiVersionMetadata item ) => item != null && items.Contains( item );

    /// <inheritdoc />
    public void CopyTo( ApiVersionMetadata[] array, int arrayIndex ) => items.CopyTo( array, arrayIndex );

    /// <inheritdoc />
    public IEnumerator<ApiVersionMetadata> GetEnumerator() => items.GetEnumerator();

    /// <inheritdoc />
    public int IndexOf( ApiVersionMetadata item ) => item == null ? -1 : items.IndexOf( item );

    /// <inheritdoc />
    public void Insert( int index, ApiVersionMetadata item ) => Insert( index, item, default );

    /// <summary>
    /// Inserts an item into the collection.
    /// </summary>
    /// <param name="index">The zero-based index where insertion takes place.</param>
    /// <param name="item">The item to insert.</param>
    /// <param name="groupName">The associated group name, if any.</param>
    public void Insert( int index, ApiVersionMetadata item, string? groupName )
    {
        items.Insert( index, item ?? throw new ArgumentNullException( nameof( item ) ) );
        groups.Insert( index, groupName );
    }

    /// <inheritdoc />
    public bool Remove( ApiVersionMetadata item )
    {
        if ( item == null )
        {
            return false;
        }

        var index = items.IndexOf( item );

        if ( index < 0 )
        {
            return false;
        }

        RemoveAt( index );
        return true;
    }

    /// <inheritdoc />
    public void RemoveAt( int index )
    {
        items.RemoveAt( index );
        groups.RemoveAt( index );
    }

    IEnumerator IEnumerable.GetEnumerator() => ( (IEnumerable) items ).GetEnumerator();

    /// <summary>
    /// Gets the group name for the item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to get the group name for.</param>
    /// <returns>The associated group name or <c>null</c>.</returns>
    /// <remarks>If the specified <paramref name="index"/> is out of range, <c>null</c>
    /// is returned.</remarks>
    public string? GroupName( int index ) =>
        index < 0 || index >= groups.Count ? default : groups[index];
}