// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Globalization;
using System.Text;

/// <content>
/// Contains additional implementation specific to .NET Standard 2.0.
/// </content>
public partial class ApiVersionFormatProvider
{
    /// <summary>
    /// Attempts to format the provided argument with the specified format and provider.
    /// </summary>
    /// <param name="destination">The format destination.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <param name="format">The format string to apply to the argument.</param>
    /// <param name="arg">The argument to format.</param>
    /// <param name="provider">The <see cref="IFormatProvider"/> used to format the argument.</param>
    /// <returns>True if formatting succeeded; otherwise, false.</returns>
    public virtual bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        object? arg,
        IFormatProvider? provider )
    {
        if ( arg is not ApiVersion value )
        {
            charsWritten = 0;
            return false;
        }

        provider = provider is null || ReferenceEquals( this, provider ) ?
                   CultureInfo.CurrentCulture :
                   provider;

        if ( format.IsEmpty )
        {
            return TryFormatAllParts( destination, out charsWritten, value, format, provider );
        }

        var writer = new FormatWriter( this, destination, value, provider );

        FormatTokenizer.Tokenize( format, ref writer );
        charsWritten = writer.Written;

        return writer.Succeeded;
    }

    /// <summary>
    /// Attempts to format all parts using the default format.
    /// </summary>
    /// <param name="destination">The format destination.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
    /// <param name="format">The format string for the group version.</param>
    /// <param name="provider">The <see cref="IFormatProvider"/> used to apply the format.</param>
    /// <returns>True if formatting succeeds; otherwise, false.</returns>
    protected virtual bool TryFormatAllParts(
        Span<char> destination,
        out int charsWritten,
        ApiVersion apiVersion,
        in ReadOnlySpan<char> format,
        IFormatProvider? provider )
    {
        if ( apiVersion == null )
        {
            charsWritten = 0;
            return false;
        }

        if ( apiVersion.GroupVersion.HasValue )
        {
            var group = apiVersion.GroupVersion.Value;

            if ( !Str.TryFormat( group, destination, out charsWritten, Str.AsSpan( GroupVersionFormat ), provider ) )
            {
                return false;
            }

            destination = Str.Substring( destination, charsWritten );
        }
        else
        {
            charsWritten = 0;
        }

        int written;

        if ( apiVersion.MajorVersion.HasValue )
        {
            if ( charsWritten > 0 )
            {
                if ( destination.Length < 2 )
                {
                    return false;
                }

                destination[0] = '.';
                destination = Str.Substring( destination, 1 );
                charsWritten++;
            }

            var value = apiVersion.MajorVersion.Value;

            if ( !Str.TryFormat( value, destination, out written, default, provider ) )
            {
                return false;
            }

            destination = Str.Substring( destination, written );
            charsWritten += written;

            if ( apiVersion.MinorVersion.HasValue )
            {
                if ( destination.Length < 2 )
                {
                    return false;
                }

                destination[0] = '.';
                destination = Str.Substring( destination, 1 );
                charsWritten++;
                value = apiVersion.MinorVersion.Value;

                if ( !Str.TryFormat( value, destination, out written, default, provider ) )
                {
                    return false;
                }

                destination = Str.Substring( destination, written );
                charsWritten += written;
            }
            else if ( Str.StringOrSpan( format ).Equals( "FF", StringComparison.Ordinal ) )
            {
                if ( destination.Length < 2 )
                {
                    return false;
                }
                else
                {
                    destination[0] = '.';
                    destination[1] = '0';
                    charsWritten += 2;
                    destination = Str.Substring( destination, 2 );
                }
            }
        }
        else if ( apiVersion.MinorVersion.HasValue )
        {
            if ( destination.Length < 3 )
            {
                return false;
            }

            destination[0] = '0';
            destination[1] = '.';
            charsWritten += 2;
            destination = Str.Substring( destination, 2 );

            var value = apiVersion.MinorVersion.Value;

            if ( !Str.TryFormat( value, destination, out written, default, provider ) )
            {
                return false;
            }

            destination = Str.Substring( destination, written );
            charsWritten += written;
        }

        if ( charsWritten > 0 && !string.IsNullOrEmpty( apiVersion.Status ) )
        {
            return TryAppendStatus( destination, ref charsWritten, apiVersion.Status );
        }

        return true;
    }

    /// <summary>
    /// Attempts to format the specified group version using the provided format.
    /// </summary>
    /// <param name="destination">The format destination.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
    /// <param name="format">The format string for the group version.</param>
    /// <param name="provider">The <see cref="IFormatProvider"/> used to apply the format.</param>
    /// <returns>True if formatting succeeds; otherwise, false.</returns>
    protected virtual bool TryFormatGroupVersionPart(
        Span<char> destination,
        out int charsWritten,
        ApiVersion apiVersion,
        in ReadOnlySpan<char> format,
        IFormatProvider? provider )
    {
        if ( apiVersion == null || format.IsEmpty )
        {
            charsWritten = 0;
            return false;
        }

        if ( !apiVersion.GroupVersion.HasValue )
        {
            charsWritten = 0;
            return true;
        }

        var groupVersion = apiVersion.GroupVersion.Value;

        switch ( format![0] )
        {
            case 'G':
                // G, GG
                if ( !Str.TryFormat( groupVersion, destination, out charsWritten, Str.AsSpan( GroupVersionFormat ), provider ) )
                {
                    return false;
                }

                // GG
                if ( format.Length == 2 )
                {
                    destination = Str.Substring( destination, charsWritten );
                    return TryAppendStatus( destination, ref charsWritten, apiVersion.Status );
                }

                return true;
            case 'M':
                // M
                // MM
                // MMM
                // MMMM*
                if ( format.Length == 1 )
                {
                    return Str.TryFormat( groupVersion.Month, destination, out charsWritten, default, provider );
                }

                break;
            case 'd':
                // d
                // dd
                // ddd
                // dddd*
                if ( format.Length == 1 )
                {
                    return Str.TryFormat( groupVersion.Day, destination, out charsWritten, default, provider );
                }

                break;
            case 'y':
                // y
                // yy
                // yyy
                // yyyy*
                if ( format.Length == 1 )
                {
                    return Str.TryFormat( groupVersion.Year % 100, destination, out charsWritten, default, provider );
                }

                break;
        }

        return Str.TryFormat( groupVersion, destination, out charsWritten, format, provider );
    }

    /// <summary>
    /// Attempts to format the specified version using the provided format.
    /// </summary>
    /// <param name="destination">The format destination.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
    /// <param name="format">The format string for the group version.</param>
    /// <param name="provider">The <see cref="IFormatProvider"/> used to apply the format.</param>
    /// <returns>True if formatting succeeds; otherwise, false.</returns>
    protected virtual bool TryFormatVersionPart(
        Span<char> destination,
        out int charsWritten,
        ApiVersion apiVersion,
        in ReadOnlySpan<char> format,
        IFormatProvider? provider )
    {
        if ( apiVersion == null || format.IsEmpty )
        {
            charsWritten = 0;
            return false;
        }

        switch ( format[0] )
        {
            case 'V':
            case 'v':
                return TryFormatVersionWithoutPadding( destination, out charsWritten, apiVersion, format, provider );
            case 'P':
            case 'p':
                return TryFormatVersionWithPadding( destination, out charsWritten, apiVersion, format, provider );
        }

        charsWritten = 0;
        return false;
    }

    /// <summary>
    /// Attempts to formats the specified status part using the provided format.
    /// </summary>
    /// <param name="destination">The format destination.</param>
    /// <param name="charsWritten">The number of characters written.</param>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to format.</param>
    /// <param name="format">The format string for the group version.</param>
    /// <param name="provider">The <see cref="IFormatProvider"/> used to apply the format.</param>
    /// <returns>True if formatting succeeds; otherwise, false.</returns>
    protected virtual bool TryFormatStatusPart(
        Span<char> destination,
        out int charsWritten,
        ApiVersion apiVersion,
        in ReadOnlySpan<char> format,
        IFormatProvider? provider )
    {
        if ( apiVersion == null )
        {
            charsWritten = 0;
            return false;
        }

        var status = apiVersion.Status;

        if ( string.IsNullOrEmpty( status ) )
        {
            charsWritten = 0;
            return false;
        }

        if ( destination.Length < status!.Length )
        {
            charsWritten = 0;
            return false;
        }

        status.AsSpan().CopyTo( destination );
        charsWritten = status.Length;
        return true;
    }

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
        in ReadOnlySpan<char> format,
        IFormatProvider? formatProvider )
    {
        ArgumentNullException.ThrowIfNull( text );
        ArgumentNullException.ThrowIfNull( apiVersion );

        if ( !apiVersion.GroupVersion.HasValue || format.IsEmpty )
        {
            return;
        }

        var groupVersion = apiVersion.GroupVersion.Value;
        Span<char> buffer = stackalloc char[16];
        int length;

        switch ( format![0] )
        {
            case 'G':
                // G, GG
                Str.TryFormat( groupVersion, buffer, out length, Str.AsSpan( GroupVersionFormat ), formatProvider );
                text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );

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
                    text.Append( groupVersion.Month.ToString( formatProvider ) );
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
                    text.Append( groupVersion.Day.ToString( formatProvider ) );
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
                    text.Append( ( groupVersion.Year % 100 ).ToString( formatProvider ) );
                    return;
                }

                break;
        }

        if ( Str.TryFormat( groupVersion, buffer, out length, format, formatProvider ) )
        {
            text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
        }
        else
        {
            text.Append( groupVersion.ToString( format.ToString(), formatProvider ) );
        }
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
        in ReadOnlySpan<char> format,
        IFormatProvider? formatProvider )
    {
        ArgumentNullException.ThrowIfNull( text );
        ArgumentNullException.ThrowIfNull( apiVersion );

        Span<char> buffer = stackalloc char[10];

        if ( apiVersion.GroupVersion.HasValue )
        {
            var group = apiVersion.GroupVersion.Value;

            if ( Str.TryFormat( group, buffer, out _, Str.AsSpan( GroupVersionFormat ), formatProvider ) )
            {
                text.Append( Str.StringOrSpan( buffer ) );
            }
            else
            {
                text.Append( group.ToString( GroupVersionFormat, formatProvider ) );
            }
        }

        if ( apiVersion.MajorVersion.HasValue )
        {
            if ( text.Length > 0 )
            {
                text.Append( '.' );
            }

            var major = apiVersion.MajorVersion.Value;

            if ( Str.TryFormat( major, buffer, out var length, default, formatProvider ) )
            {
                text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
            }
            else
            {
                text.Append( major.ToString( formatProvider ) );
            }

            if ( apiVersion.MinorVersion.HasValue )
            {
                var minor = apiVersion.MinorVersion.Value;

                text.Append( '.' );

                if ( Str.TryFormat( minor, buffer, out length, default, formatProvider ) )
                {
                    text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
                }
                else
                {
                    text.Append( minor.ToString( formatProvider ) );
                }
            }
            else if ( Str.StringOrSpan( format ).Equals( "FF", StringComparison.Ordinal ) )
            {
                text.Append( ".0" );
            }
        }
        else if ( apiVersion.MinorVersion.HasValue )
        {
            var minor = apiVersion.MinorVersion.Value;

            text.Append( "0." );

            if ( Str.TryFormat( minor, buffer, out var length, default, formatProvider ) )
            {
                text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
            }
            else
            {
                text.Append( minor.ToString( formatProvider ) );
            }
        }

        if ( text.Length > 0 && !string.IsNullOrEmpty( apiVersion.Status ) )
        {
            text.Append( '-' ).Append( apiVersion.Status );
        }
    }

    internal bool TryAppendCustomFormat(
        Span<char> destination,
        out int charsWritten,
        ApiVersion apiVersion,
        in ReadOnlySpan<char> format,
        IFormatProvider? provider )
    {
        switch ( format[0] )
        {
            case 'F':
                return TryFormatAllParts( destination, out charsWritten, apiVersion, format, provider );
            case 'G':
            case 'M':
            case 'd':
            case 'y':
                return TryFormatGroupVersionPart( destination, out charsWritten, apiVersion, format, provider );
            case 'P':
            case 'V':
            case 'p':
            case 'v':
                return TryFormatVersionPart( destination, out charsWritten, apiVersion, format, provider );
            case 'S':
                return TryFormatStatusPart( destination, out charsWritten, apiVersion, format, provider );
            default:
                charsWritten = 0;
                return false;
        }
    }

    private static bool TryFormatVersionWithoutPadding(
        Span<char> destination,
        out int charsWritten,
        ApiVersion apiVersion,
        in ReadOnlySpan<char> format,
        IFormatProvider? provider )
    {
        int value;

        if ( format.Length == 1 && format[0] == 'v' )
        {
            if ( apiVersion.MinorVersion.HasValue )
            {
                value = apiVersion.MinorVersion.Value;
                return Str.TryFormat( value, destination, out charsWritten, default, provider );
            }

            charsWritten = 0;
            return true;
        }

        if ( !apiVersion.MajorVersion.HasValue || format[0] != 'V' )
        {
            charsWritten = 0;
            return true;
        }

        // V*
        value = apiVersion.MajorVersion.Value;
        if ( !Str.TryFormat( value, destination, out charsWritten, default, provider ) )
        {
            return false;
        }

        if ( format.Length == 1 )
        {
            return true;
        }

        destination = Str.Substring( destination, charsWritten );
        var minor = apiVersion.MinorVersion ?? 0;
        int written;

        switch ( format.Length )
        {
            case 2: // VV
                if ( destination.Length < 2 )
                {
                    return false;
                }

                destination[0] = '.';
                charsWritten++;

                if ( Str.TryFormat( minor, Str.Substring( destination, 1 ), out written, default, provider ) )
                {
                    charsWritten += written;
                    return true;
                }

                return false;
            case 3: // VVV
                if ( minor > 0 )
                {
                    if ( destination.Length < 2 )
                    {
                        return false;
                    }

                    destination[0] = '.';
                    destination = Str.Substring( destination, 1 );
                    charsWritten++;

                    if ( !Str.TryFormat( minor, destination, out written, default, provider ) )
                    {
                        return false;
                    }

                    charsWritten += written;
                    destination = Str.Substring( destination, written );
                }

                return TryAppendStatus( destination, ref charsWritten, apiVersion.Status );
            case 4: // VVVV
                if ( destination.Length < 2 )
                {
                    return false;
                }

                destination[0] = '.';
                destination = Str.Substring( destination, 1 );
                charsWritten++;

                if ( !Str.TryFormat( minor, destination, out written, default, provider ) )
                {
                    return false;
                }

                charsWritten += written;
                destination = Str.Substring( destination, written );

                return TryAppendStatus( destination, ref charsWritten, apiVersion.Status );
        }

        return false;
    }

    private static bool TryFormatVersionWithPadding(
        Span<char> destination,
        out int charsWritten,
        ApiVersion apiVersion,
        in ReadOnlySpan<char> format,
        IFormatProvider? provider )
    {
        SplitFormatSpecifierWithNumber( format, provider, out var specifier, out var count );

        const string TwoDigits = "D2";
        const string LeadingZeros = "'D'0";
        string fmt;
        int value;

        // p, p(n)
        if ( specifier[0] == 'p' )
        {
            if ( apiVersion.MinorVersion.HasValue )
            {
                fmt = count.ToString( LeadingZeros, InvariantCulture );
                value = apiVersion.MinorVersion.Value;
                return Str.TryFormat( value, destination, out charsWritten, Str.AsSpan( fmt ), provider );
            }

            charsWritten = 0;
            return true;
        }

        if ( !apiVersion.MajorVersion.HasValue || format[0] != 'P' )
        {
            charsWritten = 0;
            return true;
        }

        // P, P(n)
        if ( specifier.Length == 1 )
        {
            fmt = count.ToString( LeadingZeros, InvariantCulture );
            value = apiVersion.MajorVersion.Value;
            return Str.TryFormat( value, destination, out charsWritten, Str.AsSpan( fmt ), provider );
        }

        value = apiVersion.MajorVersion.Value;

        if ( !Str.TryFormat( value, destination, out charsWritten, Str.AsSpan( TwoDigits ), provider ) )
        {
            return false;
        }

        destination = Str.Substring( destination, charsWritten );

        var minor = apiVersion.MinorVersion ?? 0;
        int written;

        switch ( format.Length )
        {
            case 2: // PP
                if ( destination.Length < 2 )
                {
                    return false;
                }

                destination[0] = '.';
                charsWritten++;

                if ( Str.TryFormat( minor, Str.Substring( destination, 1 ), out written, Str.AsSpan( TwoDigits ), provider ) )
                {
                    charsWritten += written;
                    return true;
                }

                return false;
            case 3: // PPP
                if ( minor > 0 )
                {
                    if ( destination.Length < 2 )
                    {
                        return false;
                    }

                    destination[0] = '.';
                    destination = Str.Substring( destination, 1 );
                    charsWritten++;

                    if ( !Str.TryFormat( minor, destination, out written, Str.AsSpan( TwoDigits ), provider ) )
                    {
                        return false;
                    }

                    charsWritten += written;
                }

                return TryAppendStatus( destination, ref charsWritten, apiVersion.Status );
            case 4: // PPPP
                if ( destination.Length < 2 )
                {
                    return false;
                }

                destination[0] = '.';
                destination = Str.Substring( destination, 1 );
                charsWritten++;

                if ( !Str.TryFormat( minor, destination, out written, Str.AsSpan( TwoDigits ), provider ) )
                {
                    return false;
                }

                charsWritten += written;
                destination = Str.Substring( destination, written );

                return TryAppendStatus( destination, ref charsWritten, apiVersion.Status );
        }

        return false;
    }

    private static void FormatVersionWithoutPadding(
        StringBuilder text,
        ApiVersion apiVersion,
        in ReadOnlySpan<char> format,
        IFormatProvider formatProvider )
    {
        Span<char> buffer = stackalloc char[10];
        int number;
        int length;

        if ( format.Length == 1 && format[0] == 'v' )
        {
            if ( apiVersion.MinorVersion.HasValue )
            {
                number = apiVersion.MinorVersion.Value;

                if ( Str.TryFormat( number, buffer, out length, default, formatProvider ) )
                {
                    text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
                }
                else
                {
                    text.Append( number.ToString( formatProvider ) );
                }
            }

            return;
        }

        if ( !apiVersion.MajorVersion.HasValue || format[0] != 'V' )
        {
            return;
        }

        number = apiVersion.MajorVersion.Value;

        // V*
        if ( Str.TryFormat( number, buffer, out length, default, formatProvider ) )
        {
            text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
        }
        else
        {
            text.Append( number.ToString( formatProvider ) );
        }

        if ( format.Length == 1 )
        {
            return;
        }

        number = apiVersion.MinorVersion ?? 0;

        switch ( format.Length )
        {
            case 2: // VV
                text.Append( '.' );

                if ( Str.TryFormat( number, buffer, out length, default, formatProvider ) )
                {
                    text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
                }
                else
                {
                    text.Append( number.ToString( formatProvider ) );
                }

                break;
            case 3: // VVV
                if ( number > 0 )
                {
                    text.Append( '.' );

                    if ( Str.TryFormat( number, buffer, out length, default, formatProvider ) )
                    {
                        text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
                    }
                    else
                    {
                        text.Append( number.ToString( formatProvider ) );
                    }
                }

                AppendStatus( text, apiVersion.Status );
                break;
            case 4: // VVVV
                text.Append( '.' );

                if ( Str.TryFormat( number, buffer, out length, default, formatProvider ) )
                {
                    text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
                }
                else
                {
                    text.Append( number.ToString( formatProvider ) );
                }

                AppendStatus( text, apiVersion.Status );
                break;
        }
    }

    private static void FormatVersionWithPadding(
        StringBuilder text,
        ApiVersion apiVersion,
        in ReadOnlySpan<char> format,
        IFormatProvider formatProvider )
    {
        SplitFormatSpecifierWithNumber( format, formatProvider, out var specifier, out var count );

        const string TwoDigits = "D2";
        const string LeadingZeros = "'D'0";
        Span<char> buffer = stackalloc char[Math.Max( count, 10 )];
        int number;
        int length;
        string fmt;

        // p, p(n)
        if ( specifier[0] == 'p' )
        {
            if ( apiVersion.MinorVersion.HasValue )
            {
                fmt = count.ToString( LeadingZeros, InvariantCulture );
                number = apiVersion.MinorVersion.Value;

                if ( Str.TryFormat( number, buffer, out length, Str.AsSpan( fmt ), formatProvider ) )
                {
                    text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
                }
                else
                {
                    text.Append( number.ToString( fmt, formatProvider ) );
                }
            }

            return;
        }

        if ( !apiVersion.MajorVersion.HasValue || format[0] != 'P' )
        {
            return;
        }

        number = apiVersion.MajorVersion.Value;

        // P, P(n)
        if ( specifier.Length == 1 )
        {
            fmt = count.ToString( LeadingZeros, InvariantCulture );

            if ( Str.TryFormat( number, buffer, out length, Str.AsSpan( fmt ), formatProvider ) )
            {
                text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
            }
            else
            {
                text.Append( number.ToString( fmt, formatProvider ) );
            }

            return;
        }

        if ( Str.TryFormat( number, buffer, out length, Str.AsSpan( TwoDigits ), formatProvider ) )
        {
            text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
        }
        else
        {
            text.Append( number.ToString( TwoDigits, formatProvider ) );
        }

        number = apiVersion.MinorVersion ?? 0;

        switch ( format.Length )
        {
            case 2: // PP
                text.Append( '.' );

                if ( Str.TryFormat( number, buffer, out length, Str.AsSpan( TwoDigits ), formatProvider ) )
                {
                    text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
                }
                else
                {
                    text.Append( number.ToString( TwoDigits, formatProvider ) );
                }

                break;
            case 3: // PPP
                if ( number > 0 )
                {
                    text.Append( '.' );

                    if ( Str.TryFormat( number, buffer, out length, Str.AsSpan( TwoDigits ), formatProvider ) )
                    {
                        text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
                    }
                    else
                    {
                        text.Append( number.ToString( TwoDigits, formatProvider ) );
                    }
                }

                AppendStatus( text, apiVersion.Status );
                break;
            case 4: // PPPP
                text.Append( '.' );

                if ( Str.TryFormat( number, buffer, out length, Str.AsSpan( TwoDigits ), formatProvider ) )
                {
                    text.Append( Str.StringOrSpan( Str.Truncate( buffer, length ) ) );
                }
                else
                {
                    text.Append( number.ToString( TwoDigits, formatProvider ) );
                }

                AppendStatus( text, apiVersion.Status );
                break;
        }
    }

    private static bool TryAppendStatus(
        Span<char> destination,
        ref int charsWritten,
        string? status )
    {
        if ( string.IsNullOrEmpty( status ) )
        {
            return true;
        }

        var count = status!.Length + 1;

        if ( destination.Length < count )
        {
            return false;
        }

        destination[0] = '-';
        status.AsSpan().CopyTo( Str.Substring( destination, 1 ) );
        charsWritten += count;
        return true;
    }
}