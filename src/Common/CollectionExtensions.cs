#if WEBAPI
namespace Microsoft.Web.Http
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
#if WEBAPI
    using Microsoft.Web.Http.Versioning;
#else
    using Microsoft.AspNetCore.Mvc.Versioning;
#endif
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using static System.Globalization.CultureInfo;
    using static System.String;

    static partial class CollectionExtensions
    {
#if NETAPPCORE3_1
        internal static bool TryGetValue<TKey, TValue>( this IDictionary<TKey, object?> dictionary, TKey key, [NotNullWhen(true)] out TValue value ) where TKey : notnull
#else
        internal static bool TryGetValue<TKey, TValue>( this IDictionary<TKey, object?> dictionary, TKey key, out TValue value ) where TKey : notnull
#endif
        {
            if ( dictionary.TryGetValue( key, out var val ) && val is TValue v )
            {
                value = v;
                return true;
            }

            value = default!;
            return false;
        }

        internal static List<T> AsList<T>( this IEnumerable<T> sequence ) => ( sequence as List<T> ) ?? new List<T>( sequence );

        internal static IReadOnlyList<T> ToSortedReadOnlyList<T>( this IEnumerable<T> sequence ) where T : IComparable<T>
        {
            if ( sequence is List<T> list )
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
            foreach ( var item in items )
            {
                collection.Add( item );
            }
        }

        internal static string EnsureZeroOrOneApiVersions( this ICollection<string> apiVersions )
        {
            if ( apiVersions.Count < 2 )
            {
                return apiVersions.SingleOrDefault();
            }

            var requestedVersions = Join( ", ", apiVersions.OrderBy( v => v ) );
            var message = Format( InvariantCulture, SR.MultipleDifferentApiVersionsRequested, requestedVersions );

            throw new AmbiguousApiVersionException( message, apiVersions.OrderBy( v => v ) );
        }

        internal static void UnionWith<T>( this ICollection<T> collection, IEnumerable<T> other )
        {
            if ( collection is ISet<T> set )
            {
                set.UnionWith( other );
            }
            else
            {
                foreach ( var item in other )
                {
                    if ( !collection.Contains( item ) )
                    {
                        collection.Add( item );
                    }
                }
            }
        }
    }
}