#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static partial class CollectionExtensions
    {
        internal static bool TryGetValue<TKey, TValue>( this IDictionary<TKey, object> dictionary, TKey key, out TValue value )
        {
            Contract.Requires( dictionary != null );
            Contract.Requires( key != null );

            object val;

            if ( dictionary.TryGetValue( key, out val ) && ( val is TValue ) )
            {
                value = (TValue) val;
                return true;
            }

            value = default( TValue );
            return false;
        }

        internal static List<T> AsList<T>( this IEnumerable<T> sequence ) => ( sequence as List<T> ) ?? new List<T>( sequence );

        internal static IReadOnlyList<T> ToSortedReadOnlyList<T>( this IEnumerable<T> sequence ) where T : IComparable<T>
        {
            Contract.Requires( sequence != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<T>>() != null );

            var list = sequence as List<T>;

            if ( list != null )
            {
                list.Sort();
                return list;
            }

            var array = sequence.ToArray();
            Array.Sort( array );
            return array;
        }

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
