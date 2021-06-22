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
#if WEBAPI
        internal static bool TryGetValue<TKey, TValue>( this IDictionary<TKey, object?> dictionary, TKey key, out TValue value ) where TKey : notnull
#else
        internal static bool TryGetValue<TKey, TValue>( this IDictionary<TKey, object?> dictionary, TKey key, [NotNullWhen( true )] out TValue value ) where TKey : notnull
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

        internal static List<T> AsList<T>( this IEnumerable<T> sequence ) => ( sequence as List<T> ) ?? sequence.ToList();

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
            switch ( items )
            {
                case IList<T> list:
                    for ( var i = 0; i < list.Count; i++ )
                    {
                        collection.Add( list[i] );
                    }

                    break;
                case IReadOnlyList<T> list:
                    for ( var i = 0; i < list.Count; i++ )
                    {
                        collection.Add( list[i] );
                    }

                    break;
                default:
                    foreach ( var item in items )
                    {
                        collection.Add( item );
                    }

                    break;
            }
        }

        internal static string? EnsureZeroOrOneApiVersions( this ICollection<string> apiVersions )
        {
            switch ( apiVersions.Count )
            {
                case 0:
                    return default;
                case 1:
                    var values = new string[1];
                    apiVersions.CopyTo( values, 0 );
                    return values[0];
                default:
                    var message = Format( InvariantCulture, SR.MultipleDifferentApiVersionsRequested, Join( ", ", apiVersions ) );
                    throw new AmbiguousApiVersionException( message, apiVersions );
            }
        }

        internal static void UnionWith<T>( this ICollection<T> collection, IEnumerable<T> other )
        {
            if ( collection is ISet<T> set )
            {
                set.UnionWith( other );
            }
            else
            {
                switch ( other )
                {
                    case IList<T> list:
                        for ( var i = 0; i < list.Count; i++ )
                        {
                            if ( !collection.Contains( list[i] ) )
                            {
                                collection.Add( list[i] );
                            }
                        }

                        break;
                    case IReadOnlyList<T> list:
                        for ( var i = 0; i < list.Count; i++ )
                        {
                            if ( !collection.Contains( list[i] ) )
                            {
                                collection.Add( list[i] );
                            }
                        }

                        break;
                    default:
                        foreach ( var item in other )
                        {
                            if ( !collection.Contains( item ) )
                            {
                                collection.Add( item );
                            }
                        }

                        break;
                }
            }
        }
    }
}