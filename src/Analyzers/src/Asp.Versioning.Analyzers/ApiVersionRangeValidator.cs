// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// This is a compile-time port of Asp.Versioning.ApiVersionRange.ParseRule. An analyzer cannot take a
/// dependency on Asp.Versioning.Abstractions, so the accepted syntax is mirrored here. Any change to the
/// range parser must be reflected in this type.
/// </remarks>
internal static class ApiVersionRangeValidator
{
    public static bool IsValid( string? text )
    {
        // ApiVersionRange.Parse rejects a null or empty rule before any parsing occurs
        if ( string.IsNullOrEmpty( text ) )
        {
            return false;
        }

        var end = text!.Length;
        var mid = text.IndexOf( ',' );

        if ( mid < 0 )
        {
            return IsMinimumOrExact( text, 0, end );
        }

        // only the whitespace adjacent to the separator is trimmed
        var left = TrimEnd( text, 0, mid );
        var right = TrimStart( text, mid + 1, end );

        // a rule bounded on neither side is invalid, even though both halves parse
        return TryLowerBound( text, 0, left, out var lower )
            && TryUpperBound( text, right, end, out var upper )
            && ( lower || upper );
    }

    private static bool IsMinimumOrExact( string text, int start, int end )
    {
        if ( end - start > 2 && text[start] == '[' && text[end - 1] == ']' )
        {
            start++;
            end--;
        }

        return ApiVersionValidator.IsValid( text, start, end );
    }

    private static bool TryLowerBound( string text, int start, int end, out bool bounded )
    {
        bounded = false;

        if ( start >= end || !IsLowerBound( text[start] ) )
        {
            return false;
        }

        start++;

        if ( start >= end )
        {
            return true;
        }

        bounded = ApiVersionValidator.IsValid( text, start, end );

        return bounded;
    }

    private static bool TryUpperBound( string text, int start, int end, out bool bounded )
    {
        bounded = false;

        if ( start >= end || !IsUpperBound( text[end - 1] ) )
        {
            return false;
        }

        end--;

        if ( start >= end )
        {
            return true;
        }

        bounded = ApiVersionValidator.IsValid( text, start, end );

        return bounded;
    }

    private static bool IsLowerBound( char ch ) => ch == '[' || ch == '(';

    private static bool IsUpperBound( char ch ) => ch == ']' || ch == ')';

    private static int TrimEnd( string text, int start, int end )
    {
        while ( end > start && char.IsWhiteSpace( text[end - 1] ) )
        {
            end--;
        }

        return end;
    }

    private static int TrimStart( string text, int start, int end )
    {
        while ( start < end && char.IsWhiteSpace( text[start] ) )
        {
            start++;
        }

        return start;
    }
}