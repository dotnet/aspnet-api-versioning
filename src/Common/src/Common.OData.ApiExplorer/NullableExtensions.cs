// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System;

using System.Runtime.CompilerServices;

internal static class NullableExtensions
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool Unset( this int? value ) => value.HasValue && value.Value == 0;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool NoLimitOrSome( this int? value ) => !value.HasValue || value.Value > 0;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool NoLimitOrNone( this int? value ) => !value.HasValue || value.Value <= 0;
}