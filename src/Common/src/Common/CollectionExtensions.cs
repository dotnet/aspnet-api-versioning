// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Collections.Generic;

internal static partial class CollectionExtensions
{
    extension<TKey>( IDictionary<TKey, object?> dictionary ) where TKey : notnull
    {
        internal bool TryGetValue<TValue>( TKey key, [MaybeNullWhen( false )] out TValue value )
        {
            if ( dictionary.TryGetValue( key, out var val ) && val is TValue v )
            {
                value = v;
                return true;
            }

            value = default!;
            return false;
        }
    }

    extension<T>( IEnumerable<T> sequence )
    {
        internal List<T> AsList() => ( sequence as List<T> ) ?? [.. sequence];
    }

    extension<T>( ICollection<T> collection )
    {
        internal void AddRange( IEnumerable<T> items )
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
}