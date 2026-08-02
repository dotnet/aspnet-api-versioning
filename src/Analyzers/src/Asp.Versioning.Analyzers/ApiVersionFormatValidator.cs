// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// This is a compile-time port of Asp.Versioning.FormatTokenizer. An analyzer cannot take a dependency
/// on Asp.Versioning.Abstractions, so the accepted syntax is mirrored here. Any change to the tokenizer
/// or to the supported format specifiers must be reflected in this type.
/// </remarks>
internal static class ApiVersionFormatValidator
{
    /// <summary>
    /// The largest padding count supported by ApiVersionFormatProvider.
    /// </summary>
    internal const int MaxPadding = 99;

    public static void Validate( string format, ICollection<FormatProblem> problems )
    {
        var last = format.Length - 1;

        for ( var i = 0; i < format.Length; i++ )
        {
            var ch = format[i];

            if ( ch == '\'' || ch == '"' )
            {
                if ( !TryConsumeLiteral( format, ref i ) )
                {
                    problems.Add( FormatProblem.UnterminatedLiteral( ch ) );
                    return;
                }
            }
            else if ( ch == '\\' && i < last && IsEscapable( format[i + 1] ) )
            {
                i++;
            }
            else if ( ch == '%' && i < last && IsSpecifier( format[i + 1] ) )
            {
                // a single custom format specifier is never repeated, but may still be padded
                i++;
                ConsumeSpecifier( format, ref i, repeatable: false, problems );
            }
            else if ( IsSpecifier( ch ) )
            {
                ConsumeSpecifier( format, ref i, repeatable: true, problems );
            }
        }
    }

    private static bool TryConsumeLiteral( string format, ref int i )
    {
        var delimiter = format[i];

        for ( var j = i + 1; j < format.Length; j++ )
        {
            if ( format[j] == delimiter )
            {
                i = j;
                return true;
            }
        }

        return false;
    }

    private static void ConsumeSpecifier(
        string format,
        ref int i,
        bool repeatable,
        ICollection<FormatProblem> problems )
    {
        var specifier = format[i];
        var start = i;
        var length = 1;

        if ( repeatable )
        {
            while ( i + 1 < format.Length && format[i + 1] == specifier )
            {
                i++;
                length++;
            }
        }

        var digits = i + 1;

        while ( digits < format.Length && char.IsDigit( format[digits] ) )
        {
            digits++;
        }

        if ( digits > i + 1 )
        {
            // only padding uses the count; any other specifier ignores the digits that follow it
            if ( specifier is 'P' or 'p' )
            {
                var text = format.Substring( i + 1, digits - i - 1 );

                if ( !int.TryParse( text, out var count ) || count > MaxPadding )
                {
                    problems.Add( FormatProblem.PaddingOutOfRange( text ) );
                }
            }

            i = digits - 1;
        }

        var max = MaxLength( specifier );

        if ( length > max )
        {
            problems.Add( FormatProblem.RepeatedSpecifier( format.Substring( start, length ), specifier, max, length ) );
        }
    }

    private static bool IsSpecifier( char ch ) =>
        ch is 'F' or 'G' or 'M' or 'P' or 'S' or 'V' or 'd' or 'p' or 'v' or 'y';

    private static bool IsEscapable( char ch ) =>
        ch is '\'' or '\\' || IsSpecifier( ch );

    /// <remarks>A specifier repeated beyond its maximum is silently reinterpreted rather than
    /// rejected. The year is unbounded because each additional 'y' adds a digit of padding.</remarks>
    private static int MaxLength( char specifier ) => specifier switch
    {
        'F' or 'G' => 2,
        'M' or 'P' or 'V' or 'd' => 4,
        'S' or 'p' or 'v' => 1,
        _ => int.MaxValue,
    };
}