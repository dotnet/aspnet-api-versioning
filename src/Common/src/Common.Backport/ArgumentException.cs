// Copyright (c) .NET Foundation and contributors. All rights reserved.

// REF: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/ArgumentException.cs
namespace Backport;

using System.Runtime.CompilerServices;

internal static class ArgumentException
{
    /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
    /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ThrowIfNullOrEmpty( string? argument, [CallerArgumentExpression( nameof( argument ) )] string? paramName = null )
    {
        if ( string.IsNullOrEmpty( argument ) )
        {
            ThrowNullOrEmptyException( argument, paramName );
        }
    }

    [DoesNotReturn]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ThrowNullOrEmptyException( string? argument, string? paramName )
    {
        ArgumentNullException.ThrowIfNull( argument, paramName );
        throw new System.ArgumentException( BackportSR.Argument_EmptyString, paramName );
    }
}