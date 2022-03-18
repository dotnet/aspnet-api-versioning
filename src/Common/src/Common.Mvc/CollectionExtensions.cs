// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Collections.Generic;

internal static partial class CollectionExtensions
{
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