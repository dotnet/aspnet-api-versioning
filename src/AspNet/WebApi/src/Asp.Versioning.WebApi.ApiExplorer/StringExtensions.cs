// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System;

using System.Text;

internal static class StringExtensions
{
    // REF: https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs#L890
    internal static string Replace( this string @string, string oldValue, string? newValue, StringComparison comparisonType )
    {
        if ( string.IsNullOrEmpty( oldValue ) )
        {
            throw new ArgumentNullException( nameof( oldValue ) );
        }

        var length = @string.Length;

        if ( length == 0 )
        {
            return @string;
        }

        var result = new StringBuilder( length );
        var start = 0;
        var matchLength = oldValue.Length;
        var index = @string.IndexOf( oldValue, start, comparisonType );

        while ( index >= 0 )
        {
            result.Append( @string.Substring( start, index - start ) );
            result.Append( newValue );
            start = index + matchLength;
            index = @string.IndexOf( oldValue, start, comparisonType );
        }

        if ( result.Length == 0 )
        {
            return @string;
        }

        if ( start < length )
        {
            result.Append( @string.Substring( start ) );
        }

        return result.ToString();
    }
}