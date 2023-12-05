// Copyright (c) .NET Foundation and contributors. All rights reserved.

// REF: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/ArgumentNullException.cs
namespace Backport;

using System.Runtime.CompilerServices;

internal static class ArgumentNullException
{
    /// <summary>Throws an <see cref="System.ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
    /// <param name="argument">The reference type argument to validate as non-null.</param>
    /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void ThrowIfNull( [NotNull] object? argument, [CallerArgumentExpression( nameof( argument ) )] string? paramName = null )
    {
        if ( argument is null )
        {
            throw new System.ArgumentNullException( paramName );
        }
    }
}