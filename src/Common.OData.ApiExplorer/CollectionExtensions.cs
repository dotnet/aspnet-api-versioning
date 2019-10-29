#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System.Collections.Generic;

    static class CollectionExtensions
    {
        internal static void AddRange<T>( this ICollection<T> collection, IEnumerable<T> items )
        {
            foreach ( var item in items )
            {
                collection.Add( item );
            }
        }
    }
}