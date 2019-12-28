namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Defines the behavior of a collection of registered OData routes.
    /// </summary>
    [CLSCompliant( false )]
    public interface IODataRouteCollection : IReadOnlyList<ODataRouteMapping>
    {
        /// <summary>
        /// Gets a read-only list of OData routes for the specified API version.
        /// </summary>
        /// <param name="key">The <see cref="ApiVersion">API version</see> to get the mapped routes for.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ODataRouteMapping">mapped OData routes</see>.</returns>
#pragma warning disable CA1043 // Use Integral Or String Argument For Indexers
        IReadOnlyList<ODataRouteMapping> this[ApiVersion key] { get; }
#pragma warning restore CA1043 // Use Integral Or String Argument For Indexers

        /// <summary>
        /// Gets a value indicating whether the collection contains the specified item.
        /// </summary>
        /// <param name="item">The <see cref="ODataRouteMapping">item</see> to evaluate.</param>
        /// <returns>True if the collection contains the specified item; otherwise, false.</returns>
        bool Contains( ODataRouteMapping item );

        /// <summary>
        /// Gets a value indicating whether the collection contains the specified key.
        /// </summary>
        /// <param name="key">The <see cref="ApiVersion">API version</see> to evaluate.</param>
        /// <returns>True if the collection contains the specified key; otherwise, false.</returns>
        bool ContainsKey( ApiVersion key );

        /// <summary>
        /// Attempts to retrieve the list of mapped OData routes for the specified API version.
        /// </summary>
        /// <param name="key">The <see cref="ApiVersion">API version</see> to evaluate.</param>
        /// <param name="value">A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ODataRouteMapping">mapped OData routes</see>.</param>
        /// <returns>True if the value was successfully retrieved; otherwise, false.</returns>
        bool TryGetValue( ApiVersion key, [NotNullWhen( true )] out IReadOnlyList<ODataRouteMapping>? value );

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first
        /// occurrence within the entire collection.
        /// </summary>
        /// <param name="item">The <see cref="ODataRouteMapping">item</see> to evaluate.</param>
        /// <returns>True if the collection contains the specified item; otherwise, false.</returns>
        int IndexOf( ODataRouteMapping item );

        /// <summary>
        /// Copies the entire collection to a compatible one-dimensional array, starting at the
        /// specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional array that is the destination of the elements copied
        /// from collection. The System.Array must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in array at which copying begins.</param>
        void CopyTo( ODataRouteMapping[] array, int index );
    }
}