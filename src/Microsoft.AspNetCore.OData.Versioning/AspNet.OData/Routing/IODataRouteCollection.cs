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
        /// Gets a value indicating whether the collection contains the specified item.
        /// </summary>
        /// <param name="item">The <see cref="ODataRouteMapping">item</see> to evaluate.</param>
        /// <returns>True if the collection contains the specified item; otherwise, false.</returns>
        bool Contains( ODataRouteMapping item );

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