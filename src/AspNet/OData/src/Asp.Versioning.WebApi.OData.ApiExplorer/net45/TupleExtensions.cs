// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System;

internal static class TupleExtensions
{
    extension<T1, T2>( Tuple<T1, T2> tuple )
    {
        internal void Deconstruct( out T1 item1, out T2 item2 )
        {
            item1 = tuple.Item1;
            item2 = tuple.Item2;
        }
    }
}