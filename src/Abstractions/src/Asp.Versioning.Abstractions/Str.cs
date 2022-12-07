// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1121

namespace Asp.Versioning;

using System.Runtime.CompilerServices;
#if NETSTANDARD1_0
using Text = System.String;
#else
using Text = System.ReadOnlySpan<char>;
#endif

internal static class Str
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
#if NETSTANDARD1_0
    internal static bool IsNullOrEmpty( Text? text )
#else
    internal static bool IsNullOrEmpty( Text text )
#endif
    {
#if NETSTANDARD1_0
        return string.IsNullOrEmpty( text );
#else
        return text.IsEmpty;
#endif
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Text Substring( Text text, int start )
    {
#if NETSTANDARD1_0
        return text.Substring( start );
#elif NETSTANDARD2_0
        return text.Slice( start );
#else
        return text[start..];
#endif
    }

#if NETSTANDARD2_0
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Span<char> Substring( Span<char> text, int start ) => text.Slice( start );
#elif !NETSTANDARD
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Span<char> Substring( Span<char> text, int start ) => text[start..];
#endif

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Text Substring( Text text, int start, int length )
    {
#if NETSTANDARD1_0
        return text.Substring( start, length );
#else
        return text.Slice( start, length );
#endif
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Text Slice( Text text, int start, int end )
    {
#if NETSTANDARD1_0
        return text.Substring( start, end - start );
#elif NETSTANDARD2_0
        return text.Slice( start, end - start );
#else
        return text[start..end];
#endif
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static Text Truncate( Text text, int length )
    {
#if NETSTANDARD1_0
        return text.Substring( 0, length );
#elif NETSTANDARD2_0
        return text.Slice( 0, length );
#else
        return text[..length];
#endif
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static
#if NETSTANDARD2_0
        string
#else
        Text
#endif
        StringOrSpan( Text text )
    {
#if NETSTANDARD2_0
        return text.ToString();
#else
        return text;
#endif
    }

#if NETSTANDARD2_0
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ReadOnlySpan<char> AsSpan( string text ) => text.AsSpan();
#elif !NETSTANDARD
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static ReadOnlySpan<char> AsSpan( string text ) => text;
#endif

#if NETSTANDARD2_0
    internal static bool TryFormat<T>(
        T value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format = default,
        IFormatProvider? provider = default )
        where T : IFormattable
    {
        var source = value.ToString( format.ToString(), provider ).AsSpan();
        var succeeded = source.TryCopyTo( destination );

        if ( succeeded )
        {
            charsWritten = source.Length;
        }
        else if ( ( charsWritten = destination.Length ) > 0 )
        {
            source.Slice( 0, destination.Length ).CopyTo( destination );
        }

        return succeeded;
    }
#elif !NETSTANDARD
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static bool TryFormat<T>(
        T value,
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format = default,
        IFormatProvider? provider = default )
        where T : ISpanFormattable =>
        value.TryFormat( destination, out charsWritten, format, provider );
#endif
}