// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Text;

/// <content>
/// Contains additional implementation specific to .NET Standard 1.0.
/// </content>
public partial class ApiVersionFormatProvider
{
    /// <summary>
    /// Formats the specified group version using the provided format.
    /// </summary>
    /// <param name="text">The formatted text.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
    /// <param name="format">The format string for the group version.</param>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> used to apply the format.</param>
    protected virtual void FormatGroupVersionPart(
        StringBuilder text,
        ApiVersion apiVersion,
        string? format,
        IFormatProvider? formatProvider )
    {
        ArgumentNullException.ThrowIfNull( text );
        ArgumentNullException.ThrowIfNull( apiVersion );

        if ( !apiVersion.GroupVersion.HasValue || string.IsNullOrEmpty( format ) )
        {
            return;
        }

        var groupVersion = apiVersion.GroupVersion.Value;

        switch ( format![0] )
        {
            case 'G':
                // G, GG
                text.Append( groupVersion.ToString( GroupVersionFormat, formatProvider ) );

                // GG
                if ( format.Length == 2 )
                {
                    AppendStatus( text, apiVersion.Status );
                }

                return;
            case 'M':
                // M
                // MM
                // MMM
                // MMMM*
                if ( format.Length == 1 )
                {
                    text.Append( Calendar.GetMonth( groupVersion ).ToString( formatProvider ) );
                    return;
                }

                break;
            case 'd':
                // d
                // dd
                // ddd
                // dddd*
                if ( format.Length == 1 )
                {
                    text.Append( Calendar.GetDayOfMonth( groupVersion ).ToString( formatProvider ) );
                    return;
                }

                break;
            case 'y':
                // y
                // yy
                // yyy
                // yyyy*
                if ( format.Length == 1 )
                {
                    text.Append( ( Calendar.GetYear( groupVersion ) % 100 ).ToString( formatProvider ) );
                    return;
                }

                break;
        }

        text.Append( groupVersion.ToString( format, formatProvider ) );
    }

    /// <summary>
    /// Formats all parts using the default format.
    /// </summary>
    /// <param name="text">The formatted text.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
    /// <param name="format">The format string for the API version. This parameter can be <c>null</c> or empty.</param>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> used to apply the format.</param>
    protected virtual void FormatAllParts(
        StringBuilder text,
        ApiVersion apiVersion,
        string? format,
        IFormatProvider? formatProvider )
    {
        ArgumentNullException.ThrowIfNull( text );
        ArgumentNullException.ThrowIfNull( apiVersion );

        if ( apiVersion.GroupVersion.HasValue )
        {
            text.Append( apiVersion.GroupVersion.Value.ToString( GroupVersionFormat, formatProvider ) );
        }

        if ( apiVersion.MajorVersion.HasValue )
        {
            if ( text.Length > 0 )
            {
                text.Append( '.' );
            }

            text.Append( apiVersion.MajorVersion.Value.ToString( formatProvider ) );

            if ( apiVersion.MinorVersion.HasValue )
            {
                text.Append( '.' ).Append( apiVersion.MinorVersion.Value.ToString( formatProvider ) );
            }
            else if ( format == "FF" )
            {
                text.Append( ".0" );
            }
        }
        else if ( apiVersion.MinorVersion.HasValue )
        {
            text.Append( "0." ).Append( apiVersion.MinorVersion.Value.ToString( formatProvider ) );
        }

        if ( text.Length > 0 && !string.IsNullOrEmpty( apiVersion.Status ) )
        {
            text.Append( '-' ).Append( apiVersion.Status );
        }
    }

    private static void FormatVersionWithoutPadding(
        StringBuilder text,
        ApiVersion apiVersion,
        string format,
        IFormatProvider formatProvider )
    {
        if ( format.Length == 1 && format[0] == 'v' )
        {
            if ( apiVersion.MinorVersion.HasValue )
            {
                text.Append( apiVersion.MinorVersion.Value.ToString( formatProvider ) );
            }

            return;
        }

        if ( !apiVersion.MajorVersion.HasValue || format[0] != 'V' )
        {
            return;
        }

        // V*
        text.Append( apiVersion.MajorVersion.Value.ToString( formatProvider ) );

        if ( format.Length == 1 )
        {
            return;
        }

        var minor = apiVersion.MinorVersion ?? 0;

        switch ( format.Length )
        {
            case 2: // VV
                text.Append( '.' ).Append( minor.ToString( formatProvider ) );
                break;
            case 3: // VVV
                if ( minor > 0 )
                {
                    text.Append( '.' ).Append( minor.ToString( formatProvider ) );
                }

                AppendStatus( text, apiVersion.Status );
                break;
            case 4: // VVVV
                text.Append( '.' ).Append( minor.ToString( formatProvider ) );
                AppendStatus( text, apiVersion.Status );
                break;
        }
    }

    private static void FormatVersionWithPadding(
        StringBuilder text,
        ApiVersion apiVersion,
        string format,
        IFormatProvider formatProvider )
    {
        SplitFormatSpecifierWithNumber( format, formatProvider, out var specifier, out var count );

        const string TwoDigits = "D2";
        const string LeadingZeros = "'D'0";
        string fmt;

        // p, p(n)
        if ( specifier == "p" )
        {
            if ( apiVersion.MinorVersion.HasValue )
            {
                fmt = count.ToString( LeadingZeros, InvariantCulture );
                text.Append( apiVersion.MinorVersion.Value.ToString( fmt, formatProvider ) );
            }

            return;
        }

        if ( !apiVersion.MajorVersion.HasValue || format[0] != 'P' )
        {
            return;
        }

        // P, P(n)
        if ( specifier == "P" )
        {
            fmt = count.ToString( LeadingZeros, InvariantCulture );
            text.Append( apiVersion.MajorVersion.Value.ToString( fmt, formatProvider ) );
            return;
        }

        text.Append( apiVersion.MajorVersion.Value.ToString( TwoDigits, formatProvider ) );

        var minor = apiVersion.MinorVersion ?? 0;

        switch ( format.Length )
        {
            case 2: // PP
                text.Append( '.' ).Append( minor.ToString( TwoDigits, formatProvider ) );
                break;
            case 3: // PPP
                if ( minor > 0 )
                {
                    text.Append( '.' ).Append( minor.ToString( TwoDigits, formatProvider ) );
                }

                AppendStatus( text, apiVersion.Status );
                break;
            case 4: // PPPP
                text.Append( '.' ).Append( minor.ToString( TwoDigits, formatProvider ) );
                AppendStatus( text, apiVersion.Status );
                break;
        }
    }
}