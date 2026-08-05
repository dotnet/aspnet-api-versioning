// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers;

using System.Globalization;

/// <summary>
/// Determines whether a namespace declares an API version.
/// </summary>
/// <remarks>
/// This is a compile-time port of Asp.Versioning.NamespaceParser. An analyzer cannot take a dependency
/// on Asp.Versioning.Abstractions, so the accepted syntax is mirrored here. Only whether a namespace
/// declares a version is needed, not which one, so the parsed value is discarded. Any change to the
/// parser must be reflected in this type.
/// </remarks>
public static class NamespaceVersion
{
    private const string CompactDateFormat = "yyyyMMdd";
    private const string ReadableDateFormat = "yyyy_MM_dd";

    /// <summary>
    /// Determines whether any part of a namespace declares an API version.
    /// </summary>
    /// <param name="namespace">The namespace to evaluate.</param>
    /// <returns>True if any part of the <paramref name="namespace"/> declares an API version;
    /// otherwise, false.</returns>
    public static bool IsVersioned( string? @namespace )
    {
        if ( string.IsNullOrEmpty( @namespace ) )
        {
            return false;
        }

        var start = 0;

        for ( var end = 0; end <= @namespace!.Length; end++ )
        {
            if ( end < @namespace.Length && @namespace[end] != '.' )
            {
                continue;
            }

            if ( IsVersion( @namespace.Substring( start, end - start ) ) )
            {
                return true;
            }

            start = end + 1;
        }

        return false;
    }

    /// <remarks>
    /// The accepted shape is a 'v', 'V', or '_' prefix, followed by an optional date, an optional
    /// major and minor number, and an optional status. Examples include v1, v2_0_Beta, v20180401,
    /// _2018_04_01, and _2018_04_01_1_0_Beta.
    /// </remarks>
    private static bool IsVersion( string identifier )
    {
        if ( identifier.Length == 0 )
        {
            return false;
        }

        var ch = identifier[0];

        if ( ch != 'v' && ch != 'V' && ch != '_' )
        {
            return false;
        }

        identifier = identifier.Substring( 1 );

        var dated = false;

        if ( identifier.Length >= 8 )
        {
            if ( !TryConsumeGroup( ref identifier, CompactDateFormat, length: 8, ref dated ) )
            {
                return false;
            }

            if ( !dated &&
                 identifier.Length >= 10 &&
                 !TryConsumeGroup( ref identifier, ReadableDateFormat, length: 10, ref dated ) )
            {
                return false;
            }
        }

        string status;

        if ( identifier.Length == 0 )
        {
            if ( !dated )
            {
                return false;
            }

            status = string.Empty;
        }
        else if ( TryConsumeNumber( ref identifier ) )
        {
            TryConsumeNumber( ref identifier );
            status = identifier;
        }
        else if ( !dated )
        {
            return false;
        }
        else
        {
            status = identifier;
        }

        return IsValidStatus( status );
    }

    /// <remarks>A segment that is not a date at all is not a failure, because the remainder may still
    /// be a number. A segment shaped like a date but not naming one ends the attempt.</remarks>
    private static bool TryConsumeGroup( ref string identifier, string format, int length, ref bool dated )
    {
        var segment = identifier.Substring( 0, length );

        if ( !DateTime.TryParseExact(
                segment,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _ ) )
        {
            return !IsDateLike( segment );
        }

        identifier = Advance( identifier, length );
        dated = true;

        return true;
    }

    private static bool TryConsumeNumber( ref string identifier )
    {
        var length = 0;

        while ( length < identifier.Length && char.IsDigit( identifier[length] ) )
        {
            length++;
        }

        if ( length == 0 ||
             !int.TryParse(
                 identifier.Substring( 0, length ),
                 NumberStyles.None,
                 CultureInfo.InvariantCulture,
                 out _ ) )
        {
            return false;
        }

        identifier = Advance( identifier, length );

        return true;
    }

    private static string Advance( string identifier, int length )
    {
        if ( identifier.Length == length )
        {
            return string.Empty;
        }

        if ( identifier[length] == '_' )
        {
            length++;
        }

        return identifier.Substring( length );
    }

    private static bool IsDateLike( string value )
    {
        if ( value.Length == 8 )
        {
            for ( var i = 0; i < 8; i++ )
            {
                if ( !char.IsDigit( value[i] ) )
                {
                    return false;
                }
            }

            return true;
        }

        if ( value.Length != 10 )
        {
            return false;
        }

        for ( var i = 0; i < 10; i++ )
        {
            var ch = value[i];

            switch ( i )
            {
                case 4:
                case 7:
                    if ( ch != '_' )
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

    private static bool IsValidStatus( string status )
    {
        if ( status.Length == 0 )
        {
            return true;
        }

        if ( !char.IsLetter( status[0] ) )
        {
            return false;
        }

        for ( var i = 1; i < status.Length; i++ )
        {
            var ch = status[i];

            if ( !char.IsLetterOrDigit( ch ) && ch != '.' )
            {
                return false;
            }
        }

        return true;
    }
}