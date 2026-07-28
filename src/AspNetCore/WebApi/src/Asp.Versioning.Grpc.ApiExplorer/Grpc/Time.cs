// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Grpc;

using Google.Protobuf.WellKnownTypes;
using static System.Globalization.CultureInfo;
using static System.Globalization.DateTimeStyles;
using static System.TimeSpan;

internal static class Time
{
    // the maximum supported time resolution allows a mantissa of 9. DateTime/DateTimeOffset have a maximum resolution
    // of 100ns, which has a mantissa of 7. roundtrip parsing supports rfc 3339 and does most of the work. if the
    // mantissa is greater than 7, we will need to truncate the subsecond portion and calculate nanoseconds ourself.
    public static Timestamp FromRfc3339( ReadOnlySpan<char> text )
    {
        var dto = DateTimeOffset.Parse( text, null, RoundtripKind ).ToUniversalTime();
        var index = text.IndexOf( '.' );
        var nanos = 0;

        if ( index >= 0 )
        {
            var start = index + 1;
            var end = start;

            while ( end < text.Length && char.IsDigit( text[end] ) )
            {
                end++;
            }

            var span = text[start..end];

            nanos = ToNanoseconds( span );

            if ( span.Length > 7 )
            {
                dto = new( dto.Year, dto.Month, dto.Day, dto.Hour, dto.Minute, dto.Second, Zero );
            }
        }

        return new()
        {
            Seconds = dto.ToUnixTimeSeconds(),
            Nanos = nanos,
        };
    }

    public static Duration FromSeconds( ReadOnlySpan<char> text )
    {
        var negative = false;

        if ( !text.IsEmpty && text[0] == '-' )
        {
            negative = true;
            text = text[1..];
        }

        if ( text.IsEmpty || text[^1] != 's' )
        {
            throw new FormatException( "Duration must end with 's'." );
        }

        text = text[..^1];

        var index = text.IndexOf( '.' );
        long seconds;
        var nanos = 0;

        if ( index < 0 )
        {
            seconds = long.Parse( text, InvariantCulture );
        }
        else
        {
            seconds = long.Parse( text[..index], InvariantCulture );
            nanos = ToNanoseconds( text[( index + 1 )..] );
        }

        if ( negative )
        {
            seconds = -seconds;
            nanos = -nanos;
        }

        return new()
        {
            Seconds = seconds,
            Nanos = nanos,
        };
    }

    private static int ToNanoseconds( ReadOnlySpan<char> text )
    {
        if ( text.Length > 9 )
        {
            text = text[..9];
        }

        var subsecs = int.Parse( text, InvariantCulture );
        var scale = 9 - text.Length;

        return subsecs * (int) Math.Pow( 10, scale );
    }
}