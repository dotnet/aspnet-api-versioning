// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Runtime.CompilerServices;

internal static class Array
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T[] Empty<T>() => new T[0];
}