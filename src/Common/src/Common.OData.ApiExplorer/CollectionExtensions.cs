// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Collections.Generic;

internal static class CollectionExtensions
{
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