#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    static class CollectionExtensions
    {
        internal static void AddRange<T>( this ICollection<T> collection, IEnumerable<T> items )
        {
            Contract.Requires( collection != null );
            Contract.Requires( items != null );

            foreach ( var item in items )
            {
                collection.Add( item );
            }
        }
    }
}