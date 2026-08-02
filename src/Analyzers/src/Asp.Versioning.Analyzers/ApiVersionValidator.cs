// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

using System.Globalization;

/// <remarks>
/// This is a compile-time port of Asp.Versioning.ApiVersionParser.TryParse. An analyzer cannot take a
/// dependency on Asp.Versioning.Abstractions, so the accepted syntax is mirrored here. Any change to the
/// parser must be reflected in this type.
/// </remarks>
internal static class ApiVersionValidator
{
    private const string GroupVersionFormat = "yyyy-MM-dd";
    private const int GroupVersionLength = 10;
    private const int MinYear = 1;
    private const int MaxYear = 9999;
    private const int MonthsPerYear = 12;
    private const int MaxDaysPerMonth = 31;

    public static bool IsValid( string? text ) =>
        !string.IsNullOrEmpty( text ) && IsValid( text!, 0, text!.Length );

    public static bool IsValidNumber( double version ) =>
        version >= 0d && !double.IsNaN( version ) && !double.IsInfinity( version );

    public static bool IsValidNumber( int version ) => version >= 0;

    public static bool IsValidYear( int year ) => year is >= MinYear and <= MaxYear;

    public static bool IsValidMonth( int month ) => month is >= 1 and <= MonthsPerYear;

    public static bool IsValidDay( int day ) => day is >= 1 and <= MaxDaysPerMonth;

    /// <remarks>The individual components are assumed to already be within range. The Gregorian
    /// calendar is assumed, which is the calendar DateOnly and DateTime are composed from.</remarks>
    public static bool IsValidDate( int year, int month, int day ) =>
        day <= DateTime.DaysInMonth( year, month );

    public static bool IsValidStatus( string? status ) =>
        status is null || IsValidStatus( status, 0, status.Length );

    public static bool IsValid( string text, int start, int end )
    {
        if ( start >= end )
        {
            return false;
        }

        if ( end - start >= GroupVersionLength )
        {
            if ( DateTime.TryParseExact(
                    text.Substring( start, GroupVersionLength ),
                    GroupVersionFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _ ) )
            {
                var remaining = end - start - GroupVersionLength;

                if ( remaining == 0 )
                {
                    return true;
                }

                if ( remaining > 1 )
                {
                    switch ( text[start + GroupVersionLength] )
                    {
                        case '.':
                            start += GroupVersionLength + 1;
                            break;
                        case '-':
                            return IsValidStatus( text, start + GroupVersionLength + 1, end );
                    }
                }
                else
                {
                    return false;
                }
            }
            else if ( IsDateLike( text, start ) )
            {
                return false;
            }
        }

        var index = IndexOf( text, '-', start, end );

        if ( index > start )
        {
            if ( !IsValidStatus( text, index + 1, end ) )
            {
                return false;
            }

            end = index;
        }

        index = IndexOf( text, '.', start, end );

        if ( index > start )
        {
            return IsVersionNumber( text, start, index )
                && IsVersionNumber( text, index + 1, end );
        }

        return IsVersionNumber( text, start, end );
    }

    private static bool IsDateLike( string text, int start )
    {
        for ( var i = 0; i < GroupVersionLength; i++ )
        {
            var ch = text[start + i];

            switch ( i )
            {
                case 4:
                case 7:
                    if ( ch != '-' )
                    {
                        return false;
                    }

                    break;
                default:
                    if ( !char.IsDigit( ch ) )
                    {
                        return false;
                    }

                    break;
            }
        }

        return true;
    }

    private static bool IsValidStatus( string text, int start, int end )
    {
        if ( start >= end )
        {
            return true;
        }

        if ( !char.IsLetter( text[start] ) )
        {
            return false;
        }

        for ( var i = start + 1; i < end; i++ )
        {
            var ch = text[i];

            if ( !char.IsLetterOrDigit( ch ) && ch != '.' )
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsVersionNumber( string text, int start, int end )
    {
        var length = end - start;

        return length > 0
            && int.TryParse(
                text.Substring( start, length ),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out _ );
    }

    private static int IndexOf( string text, char value, int start, int end )
    {
        for ( var i = start; i < end; i++ )
        {
            if ( text[i] == value )
            {
                return i;
            }
        }

        return -1;
    }
}