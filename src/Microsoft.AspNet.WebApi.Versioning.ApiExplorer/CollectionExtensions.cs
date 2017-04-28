namespace Microsoft.Web.Http
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    static class CollectionExtensions
    {
        internal static int IndexOf<TItem>( this IEnumerable<TItem> sequence, TItem item, IEqualityComparer<TItem> comparer )
        {
            Contract.Requires( sequence != null );
            Contract.Requires( comparer != null );
            Contract.Ensures( Contract.Result<int>() >= -1 );

            var index = 0;

            foreach ( var element in sequence )
            {
                if ( comparer.Equals( element, item ) )
                {
                    return index;
                }

                index++;
            }

            return -1;
        }
    }
}