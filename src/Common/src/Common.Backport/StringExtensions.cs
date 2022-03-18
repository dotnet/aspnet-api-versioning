// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System;

using System.Text;

internal static class StringExtensions
{
    public static bool Contains( this string @string, string text, StringComparison comparison ) =>
        @string.IndexOf( text, comparison ) >= 0;

    public static string Replace( this string @string, string oldValue, string newValue, StringComparison comparison )
    {
        if ( string.IsNullOrEmpty( @string ) || string.IsNullOrEmpty( oldValue ) )
        {
            return @string;
        }

        switch ( comparison )
        {
            case StringComparison.Ordinal:
            case StringComparison.CurrentCulture:
#if NETSTANDARD2_0_OR_GREATER
            case StringComparison.InvariantCulture:
#endif
                return @string.Replace( oldValue, newValue );
        }

        const int NotFound = -1;
        var result = new StringBuilder( @string.Length );
        var hasReplacement = !string.IsNullOrEmpty( @newValue );
        var startSearchFromIndex = 0;
        int foundAt;

        while ( ( foundAt = @string.IndexOf( oldValue, startSearchFromIndex, comparison ) ) != NotFound )
        {
            var @charsUntilReplacment = foundAt - startSearchFromIndex;
            var matched = @charsUntilReplacment > 0;

            if ( matched )
            {
                result.Append( @string, startSearchFromIndex, @charsUntilReplacment );
            }

            if ( hasReplacement )
            {
                result.Append( @newValue );
            }

            startSearchFromIndex = foundAt + oldValue.Length;

            if ( startSearchFromIndex == @string.Length )
            {
                return result.ToString();
            }
        }

        var @charsUntilStringEnd = @string.Length - startSearchFromIndex;
        result.Append( @string, startSearchFromIndex, @charsUntilStringEnd );

        return result.ToString();
    }
}