﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Collections.Generic;

internal static class CollectionExtensions
{
    internal static int IndexOf<TItem>( this IEnumerable<TItem> sequence, TItem item, IEqualityComparer<TItem> comparer )
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