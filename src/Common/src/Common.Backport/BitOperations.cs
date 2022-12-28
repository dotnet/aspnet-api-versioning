// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079

// REF: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Numerics/BitOperations.cs
#pragma warning disable CS3019 // CLS compliance checking will not be performed because it is not visible from outside this assembly

namespace System.Numerics;

using System.Runtime.CompilerServices;

internal static class BitOperations
{
    /// <summary>
    /// Rotates the specified value left by the specified number of bits.
    /// Similar in behavior to the x86 instruction ROL.
    /// </summary>
    /// <param name="value">The value to rotate.</param>
    /// <param name="offset">The number of bits to rotate by.
    /// Any value outside the range [0..31] is treated as congruent mod 32.</param>
    /// <returns>The rotated value.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    [CLSCompliant( false )]
    public static uint RotateLeft( uint value, int offset )
        => ( value << offset ) | ( value >> ( 32 - offset ) );
}