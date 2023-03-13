// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Collections.Generic;

internal static partial class CollectionExtensions
{
    internal static bool TryGetValue<TKey, TValue>(
        this IDictionary<TKey, object?> dictionary,
        TKey key,
        [MaybeNullWhen( false )] out TValue value )
        where TKey : notnull
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
}