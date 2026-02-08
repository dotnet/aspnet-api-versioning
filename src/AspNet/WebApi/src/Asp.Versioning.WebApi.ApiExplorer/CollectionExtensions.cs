// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Collections.Generic;

internal static class CollectionExtensions
{
    extension<T>( IEnumerable<T> sequence )
    {
        internal int IndexOf( T item, IEqualityComparer<T> comparer )
        {
            var index = 0;

            foreach ( var element in sequence )
            {
                if ( comparer.Equals( element, item ) )
                {
                    return index;
                }

                index++;
            }

            return -1;
        }
    }
}