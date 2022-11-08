// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1121

namespace Asp.Versioning;

using static System.Char;
#if NETSTANDARD1_0
using Text = System.String;
#else
using Text = System.ReadOnlySpan<char>;
#endif

internal static class FormatTokenizer
{
    private static bool IsLiteralDelimiter( char ch ) => ch == '\'' || ch == '\"';

    private static bool IsFormatSpecifier( in char ch ) => ch switch
    {
        'F' or 'G' or 'M' or 'P' or 'S' or 'V' or 'd' or 'p' or 'v' or 'y' => true,
        _ => false,
    };

    private static bool IsEscapeSequence( Text sequence )
    {
        if ( sequence.Length != 2 || sequence[0] != '\\' )
        {
            return false;
        }

        return sequence[1] switch
        {
            '\'' or '\\' or 'F' or 'G' or 'M' or 'P' or 'S'
                 or 'V' or 'd' or 'p' or 'v' or 'y' => true,
            _ => false,
        };
    }

    private static bool IsSingleCustomFormatSpecifier( Text sequence )
    {
        if ( sequence.Length != 2 || sequence[0] != '%' )
        {
            return false;
        }

        return sequence[1] switch
        {
            'F' or 'G' or 'M' or 'P' or 'S' or 'V' or 'd' or 'v' or 'p' or 'y' => true,
            _ => false,
        };
    }

    private static void EnsureCurrentLiteralSequenceTerminated(
        in Text format,
        ref FormatWriter writer,
        in int start,
        ref int length )
    {
        if ( length <= 0 )
        {
            return;
        }

        writer.Write( new FormatToken( Str.Substring( format, start, length ), literal: true ) );
        length = 0;
    }

    private static void ConsumeLiteral(
        in Text format,
        ref FormatWriter writer,
        ref int i,
        ref int length )
    {
#if NETSTANDARD1_0
        var delimiter = format[i];
#else
        ref readonly var delimiter = ref format[i];
#endif

        EnsureCurrentLiteralSequenceTerminated( format, ref writer, in i, ref length );

        if ( ++i >= format.Length )
        {
            throw new FormatException( SR.InvalidFormatString );
        }

        var start = i;
#if NETSTANDARD1_0
        var current = format[i];
#else
        ref readonly var current = ref format[i];
#endif

        for ( ; i < format.Length; i++ )
        {
#if NETSTANDARD1_0
            current = format[i];
#else
            current = ref format[i];
#endif

            if ( current == delimiter )
            {
                break;
            }
            else
            {
                length++;
            }
        }

        if ( current != delimiter )
        {
            throw new FormatException( SR.InvalidFormatString );
        }

        writer.Write( new FormatToken( Str.Substring( format, start, length ), literal: true ) );
        length = 0;
    }

    private static void ConsumeEscapeSequence(
        in Text format,
        ref FormatWriter writer,
        ref int i,
        ref int length )
    {
        EnsureCurrentLiteralSequenceTerminated( format, ref writer, in i, ref length );
        writer.Write( new FormatToken( Str.Substring( format, ++i, 1 ), literal: true ) );
        length = 0;
    }

    private static void ConsumeSingleCustomFormat(
        in Text format,
        ref FormatWriter writer,
        ref int i,
        ref int length )
    {
        EnsureCurrentLiteralSequenceTerminated( format, ref writer, in i, ref length );

        var start = ++i;
        var end = start + 1;

        for ( ; end < format.Length; end++ )
        {
            if ( !IsDigit( format[end] ) )
            {
                break;
            }
        }

        length = end - start;
        writer.Write( new FormatToken( Str.Substring( format, start, length ) ) );
        length = 0;
    }

    private static void ConsumeCustomFormat(
        in Text format,
        ref FormatWriter writer,
        ref int i,
        ref int length )
    {
        EnsureCurrentLiteralSequenceTerminated( format, ref writer, in i, ref length );

        var start = i;
#if NETSTANDARD1_0
        char ch;
        var previous = format[i];
#else
        ref readonly var ch = ref format[i];
        ref readonly var previous = ref format[i];
#endif

        for ( ++i, ++length; i < format.Length; i++ )
        {
#if NETSTANDARD1_0
            ch = format[i];
#else
            ch = ref format[i];
#endif
            if ( ch == previous )
            {
                length++;
            }
            else
            {
                break;
            }
        }

        for ( ; i < format.Length; i++ )
        {
#if NETSTANDARD1_0
            ch = format[i];
#else
            ch = ref format[i];
#endif
            if ( IsDigit( ch ) )
            {
                length++;
            }
            else
            {
                break;
            }
        }

        writer.Write( new FormatToken( Str.Substring( format, start, length ) ) );
        length = 0;

        if ( i != format.Length )
        {
            --i;
        }
    }

    internal static void Tokenize( in Text format, scoped ref FormatWriter writer )
    {
        var count = format.Length;
        var last = count - 1;
        var length = 0;

        for ( var i = 0; i < count; i++ )
        {
#if NETSTANDARD1_0
            var ch = format[i];
#else
            ref readonly var ch = ref format[i];
#endif

            if ( IsLiteralDelimiter( ch ) )
            {
                ConsumeLiteral( format, ref writer, ref i, ref length );
            }
            else if ( ( ch == '\\' ) &&
                      ( i < last ) &&
                      IsEscapeSequence( Str.Substring( format, i, 2 ) ) )
            {
                ConsumeEscapeSequence( format, ref writer, ref i, ref length );
            }
            else if ( ( ch == '%' ) &&
                      ( i < last ) &&
                      IsSingleCustomFormatSpecifier( Str.Substring( format, i, 2 ) ) )
            {
                ConsumeSingleCustomFormat( format, ref writer, ref i, ref length );
            }
            else if ( IsFormatSpecifier( ch ) )
            {
                ConsumeCustomFormat( format, ref writer, ref i, ref length );
            }
            else
            {
                writer.Write( ch );
            }
#if !NETSTANDARD1_0
            if ( !writer.Succeeded )
            {
                break;
            }
#endif
        }
    }
}