// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static System.Globalization.CultureInfo;

internal static class PascalCase
{
    public static string Format( string text )
    {
        if ( string.IsNullOrEmpty( text ) )
        {
            return text;
        }

        Span<char> output = stackalloc char[text.Length];
        var j = 0;
        var capitalize = true;

        for ( var i = 0; i < text.Length; i++ )
        {
            if ( char.IsLower( text[i] ) )
            {
                if ( capitalize )
                {
                    output[j++] = char.ToUpper( text[i], InvariantCulture );
                    capitalize = false;
                }
                else
                {
                    output[j++] = text[i];
                }
            }
            else if ( char.IsUpper( text[i] ) )
            {
                output[j++] = i == 0 && !capitalize ?
                    char.ToLower( text[i], InvariantCulture ) :
                    text[i];

                capitalize = false;
            }
            else if ( char.IsDigit( text[i] ) )
            {
                output[j++] = text[i];
                capitalize = true;
            }
            else
            {
                capitalize = true;
            }
        }

        // add a trailing '_' if the name should be altered
        if ( text.Length > 0 && text[^1] == '#' )
        {
            output[j++] = '_';
        }

        // if we skipped any characters (ex: '.'), the final length will be shorter
        return new string( output[..j] );
    }
}