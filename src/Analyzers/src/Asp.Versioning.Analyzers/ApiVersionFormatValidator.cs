// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

/// <remarks>
/// Whether a format can be applied is decided by applying it, which is what the rule does. This describes what is
/// wrong with one that cannot, because the failure a format raises names no part of the format, and it reports the
/// repetition the format provider accepts but does not act on. Being wrong here costs a less specific message or a
/// missing suggestion; it cannot make a format read as valid when it is not, or the reverse.
/// </remarks>
internal static class ApiVersionFormatValidator
{
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

                if ( !int.TryParse( text, out var count ) || count > ApiVersionFormatProvider.MaxPadding )
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