// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Collections.Generic;

internal static class CollectionExtensions
{
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