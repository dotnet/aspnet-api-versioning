// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

internal static class TupleExtensions
{
    internal static void Deconstruct<T1, T2>( this Tuple<T1, T2> tuple, out T1 item1, out T2 item2 )
    {
        item1 = tuple.Item1;
        item2 = tuple.Item2;
    }

    internal static void Deconstruct<T1, T2, T3>( this Tuple<T1, T2, T3> tuple, out T1 item1, out T2 item2, out T3 item3 )
    {
        item1 = tuple.Item1;
        item2 = tuple.Item2;
        item3 = tuple.Item3;
    }
}