// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1121

namespace Asp.Versioning;

using System.Globalization;
using System.Runtime.CompilerServices;
using static Asp.Versioning.ApiVersionFormatProvider;
#if NETSTANDARD
using DateOnly = System.DateTime;
#endif
#if NETSTANDARD1_0
using Text = System.String;
#else
using Text = System.ReadOnlySpan<char>;
#endif

/// <summary>
/// Represents the default API version parser.
/// </summary>
public class ApiVersionParser : IApiVersionParser
{
    private static ApiVersionParser? @default;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionParser"/> class.
    /// </summary>
    public ApiVersionParser() => FormatProvider = CultureInfo.InvariantCulture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionParser"/> class.
    /// </summary>
    /// <param name="formatProvider">The associated format provider.</param>
    public ApiVersionParser( IFormatProvider formatProvider ) => FormatProvider = formatProvider;

    /// <summary>
    /// Gets the default API version parser.
    /// </summary>
    /// <value>The default API version parser.</value>
    public static ApiVersionParser Default => @default ??= new();

    /// <summary>
    /// Gets the format provider associated with the parser.
    /// </summary>
    /// <value>The associated format provider.</value>
    protected IFormatProvider FormatProvider { get; }

    /// <inheritdoc />
#if NETSTANDARD1_0
    public virtual ApiVersion Parse( Text? text )
#else
    public virtual ApiVersion Parse( Text text )
#endif
    {
        if ( Str.IsNullOrEmpty( text ) )
        {
            throw InvalidFormat();
        }

        var group = default( DateOnly? );
        Text segment;

        if ( text!.Length >= 10 )
        {
            segment = Str.Truncate( text, 10 );

            if ( DateOnly.TryParseExact(
                Str.StringOrSpan( segment ),
                GroupVersionFormat,
                FormatProvider,
                DateTimeStyles.None,
                out var date ) )
            {
                if ( text.Length == 10 )
                {
                    text = default;
                }
                else if ( text.Length > 11 )
                {
                    switch ( text[10] )
                    {
                        case '.':
                            text = Str.Substring( text, 11 );
                            break;
                        case '-':
                            segment = Str.Substring( text, 11 );

                            if ( ApiVersion.IsValidStatus( segment ) )
                            {
                                return new( date, status: segment.ToString() );
                            }

                            throw InvalidStatus( segment.ToString() );
                    }
                }
                else
                {
                    throw InvalidFormat();
                }

                group = date;
            }
            else if ( IsDateLike( segment ) )
            {
                throw InvalidGroupVersion( segment.ToString() );
            }
        }

        int? major;
        int? minor;
        string? status;

        if ( Str.IsNullOrEmpty( text ) )
        {
            major = default;
            minor = default;
            status = default;
        }
        else
        {
            var index = text!.IndexOf( '-' );

            if ( index > 0 )
            {
                segment = Str.Substring( text, index + 1 );

                if ( !ApiVersion.IsValidStatus( segment ) )
                {
                    throw InvalidStatus( segment.ToString() );
                }

                status = segment.ToString();
                text = Str.Truncate( text, index );
            }
            else
            {
                status = default;
            }

            index = text.IndexOf( '.' );

            if ( index > 0 )
            {
                if ( !int.TryParse(
                    Str.StringOrSpan( Str.Truncate( text, index ) ),
                    NumberStyles.Integer,
                    FormatProvider,
                    out var num ) )
                {
                    throw InvalidFormat();
                }

                major = num;

                if ( !int.TryParse(
                    Str.StringOrSpan( Str.Substring( text, index + 1 ) ),
                    NumberStyles.Integer,
                    FormatProvider,
                    out num ) )
                {
                    throw InvalidFormat();
                }

                minor = num;
            }
            else
            {
                if ( !int.TryParse(
                    Str.StringOrSpan( text ),
                    NumberStyles.Integer,
                    FormatProvider,
                    out var num ) )
                {
                    throw InvalidFormat();
                }

                major = num;
                minor = default;
            }
        }

        return new( group, major, minor, status );
    }

    /// <inheritdoc />
#if NETSTANDARD1_0
    public virtual bool TryParse( Text? text, out ApiVersion apiVersion )
#else
    public virtual bool TryParse( Text text, [MaybeNullWhen( false )] out ApiVersion apiVersion )
#endif
    {
        if ( Str.IsNullOrEmpty( text ) )
        {
            apiVersion = default!;
            return false;
        }

        var group = default( DateOnly? );
        Text segment;

        if ( text!.Length >= 10 )
        {
            segment = Str.Truncate( text, 10 );

            if ( DateOnly.TryParseExact(
                Str.StringOrSpan( segment ),
                GroupVersionFormat,
                FormatProvider,
                DateTimeStyles.None,
                out var date ) )
            {
                if ( text.Length == 10 )
                {
                    text = default;
                }
                else if ( text.Length > 11 )
                {
                    switch ( text[10] )
                    {
                        case '.':
                            text = Str.Substring( text, 11 );
                            break;
                        case '-':
                            segment = Str.Substring( text, 11 );

                            if ( ApiVersion.IsValidStatus( segment ) )
                            {
                                apiVersion = new( date, status: segment.ToString() );
                                return true;
                            }

                            apiVersion = default!;
                            return false;
                    }
                }
                else
                {
                    apiVersion = default!;
                    return false;
                }

                group = date;
            }
            else if ( IsDateLike( segment ) )
            {
                apiVersion = default!;
                return false;
            }
        }

        int? major;
        int? minor;
        string? status;

        if ( Str.IsNullOrEmpty( text ) )
        {
            major = default;
            minor = default;
            status = default;
        }
        else
        {
            var index = text!.IndexOf( '-' );

            if ( index > 0 )
            {
                segment = Str.Substring( text, index + 1 );

                if ( !ApiVersion.IsValidStatus( segment ) )
                {
                    apiVersion = default!;
                    return false;
                }

                status = segment.ToString();
                text = Str.Truncate( text, index );
            }
            else
            {
                status = default;
            }

            index = text.IndexOf( '.' );

            if ( index > 0 )
            {
                if ( !int.TryParse(
                    Str.StringOrSpan( Str.Truncate( text, index ) ),
                    NumberStyles.Integer,
                    FormatProvider,
                    out var num ) )
                {
                    apiVersion = default!;
                    return false;
                }

                major = num;

                if ( !int.TryParse(
                    Str.StringOrSpan( Str.Substring( text, index + 1 ) ),
                    NumberStyles.Integer,
                    FormatProvider,
                    out num ) )
                {
                    apiVersion = default!;
                    return false;
                }

                minor = num;
            }
            else
            {
                if ( !int.TryParse(
                    Str.StringOrSpan( text ),
                    NumberStyles.Integer,
                    FormatProvider,
                    out var num ) )
                {
                    apiVersion = default!;
                    return false;
                }

                major = num;
                minor = default;
            }
        }

        apiVersion = new( group, major, minor, status );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static FormatException InvalidFormat() => new( SR.ApiVersionInvalidFormat );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static FormatException InvalidGroupVersion( string value ) =>
        new( string.Format( CultureInfo.CurrentCulture, Format.ApiVersionBadGroupVersion, value ) );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static FormatException InvalidStatus( string value ) =>
        new( string.Format( CultureInfo.CurrentCulture, Format.ApiVersionBadStatus, value ) );

    private static bool IsDateLike( Text value )
    {
        if ( value.Length != 10 )
        {
            return false;
        }

        for ( var i = 0; i < 10; i++ )
        {
#if NETSTANDARD1_0
            var ch = value[i];
#else
            ref readonly var ch = ref value[i];
#endif
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
}