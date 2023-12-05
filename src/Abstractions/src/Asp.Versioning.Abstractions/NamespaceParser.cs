// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0079
#pragma warning disable SA1114
#pragma warning disable SA1121

namespace Asp.Versioning;

using System.Globalization;
#if NETSTANDARD
using DateOnly = System.DateTime;
using Text = System.String;
#else
using Text = System.ReadOnlySpan<char>;
#endif

/// <summary>
/// Represents API version parser from a type namespace.
/// </summary>
public class NamespaceParser
{
    private const string CompactDateFormat = "yyyyMMdd";
    private const string ReadableDateFormat = "yyyy_MM_dd";
    private static NamespaceParser? @default;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamespaceParser"/> class.
    /// </summary>
    public NamespaceParser() => FormatProvider = CultureInfo.InvariantCulture;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamespaceParser"/> class.
    /// </summary>
    /// <param name="formatProvider">The associated format provider.</param>
    public NamespaceParser( IFormatProvider formatProvider ) => FormatProvider = formatProvider;

    /// <summary>
    /// Gets the default namespace parser.
    /// </summary>
    /// <value>The default namespace parser.</value>
    public static NamespaceParser Default => @default ??= new();

    /// <summary>
    /// Gets the format provider associated with the parser.
    /// </summary>
    /// <value>The associated format provider.</value>
    protected IFormatProvider FormatProvider { get; }

    /// <summary>
    /// Parses a list of API version versions from the specified namespace.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> whose namespace to parse.</param>
    /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API verisons</see>.</returns>
    public IReadOnlyList<ApiVersion> Parse( Type type )
    {
        ArgumentNullException.ThrowIfNull( type );

        if ( string.IsNullOrEmpty( type.Namespace ) )
        {
            return Array.Empty<ApiVersion>();
        }

#if NETSTANDARD
        var text = type.Namespace;
#else
        var text = type.Namespace.AsSpan();
#endif
        var start = 0;
        var end = start;
        var version = default( ApiVersion );
        var versions = default( List<ApiVersion> );
        ApiVersion? result;

        for ( ; end < text.Length; end++ )
        {
            if ( text[end] != '.' )
            {
                continue;
            }

            if ( TryParse(
#if NETSTANDARD
                text.Substring( start, end - start ),
#else
                text[start..end],
#endif
                out result ) && result is not null )
            {
                if ( version is null )
                {
                    version = result;
                }
                else if ( versions is null )
                {
                    versions = [version, result];
                }
                else
                {
                    versions.Add( result );
                }
            }

            start = end + 1;
        }

        if ( TryParse(
#if NETSTANDARD
            text.Substring( start, end - start ),
#else

            text[start..end],
#endif
            out result ) && result is not null )
        {
            if ( version is null )
            {
                return new[] { result };
            }
            else if ( versions is null )
            {
                return new[] { version, result };
            }
            else
            {
                versions.Add( result );
                return versions;
            }
        }

        if ( version is null )
        {
            return Array.Empty<ApiVersion>();
        }
        else if ( versions is null )
        {
            return new[] { version };
        }

        return versions;
    }

    /// <summary>
    /// Attempts to parse an API version from the specified namespace identifier.
    /// </summary>
    /// <param name="identifier">The namespace identifier to parse.</param>
    /// <param name="apiVersion">The parsed <see cref="ApiVersion">API version</see> or <c>null</c>.</param>
    /// <returns>True if parsing is successful; otherwise, false.</returns>
    protected virtual bool TryParse( Text identifier, out ApiVersion? apiVersion )
    {
#if NETSTANDARD
        if ( string.IsNullOrEmpty( identifier ) )
#else
        if ( identifier.IsEmpty )
#endif
        {
            apiVersion = default;
            return false;
        }

        // 'v' | 'V' : [<year> : ['_'] : <month> : ['_'] : <day>] : ['_'] : [<major> ['_' : <minor>]] : ['_'] : [<status>]
        //
        // - v1
        // - v1_1
        // - v2_0_Beta
        // - v20180401
        // - v2018_04_01_1_1_Beta
        var ch = identifier[0];

        if ( ch != 'v' && ch != 'V' )
        {
            apiVersion = default;
            return false;
        }

#if NETSTANDARD
        identifier = identifier.Substring( 1 );
#else
        identifier = identifier[1..];
#endif
        var group = default( DateOnly? );

        if ( identifier.Length >= 8 )
        {
            if ( !TryConsumeGroup( ref identifier, CompactDateFormat, length: 8, out group ) )
            {
                apiVersion = default;
                return false;
            }

            if ( group is null &&
                 identifier.Length >= 10 &&
                 !TryConsumeGroup( ref identifier, ReadableDateFormat, length: 10, out group ) )
            {
                apiVersion = default;
                return false;
            }
        }

        int? major;
        int? minor;
        string? status;

#if NETSTANDARD
        if ( string.IsNullOrEmpty( identifier ) )
#else
        if ( identifier.IsEmpty )
#endif
        {
            if ( group is null )
            {
                apiVersion = default;
                return false;
            }

            major = default;
            minor = default;
            status = default;
        }
        else
        {
            if ( TryConsumeNumber( ref identifier, out major ) )
            {
                TryConsumeNumber( ref identifier, out minor );
                status = identifier.ToString();
            }
            else if ( group is null )
            {
                apiVersion = default;
                return false;
            }
            else
            {
                minor = default;
                status = identifier.ToString();
            }
        }

        if ( !ApiVersion.IsValidStatus( status ) )
        {
            apiVersion = default;
            return false;
        }

        apiVersion = new( group, major, minor, status );
        return true;
    }

    private static bool IsDateLike( Text value )
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

        if ( value.Length == 10 )
        {
            for ( var i = 0; i < 10; i++ )
            {
#if NETSTANDARD
                var ch = value[i];
#else
                ref readonly var ch = ref value[i];
#endif
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
        }

        return false;
    }

    private bool TryConsumeGroup( ref Text identifier, string format, int length, out DateOnly? group )
    {
#if NETSTANDARD
        var segment = identifier.Substring( 0, length );
#else
        var segment = identifier[..length];
#endif

        if ( !DateOnly.TryParseExact( segment, format, FormatProvider, DateTimeStyles.None, out var date ) )
        {
            group = default;
            return !IsDateLike( segment );
        }

        if ( identifier.Length == length )
        {
#if NETSTANDARD
            identifier = string.Empty;
#else
            identifier = default;
#endif
        }
        else
        {
            if ( identifier[length] == '_' )
            {
                length++;
            }
#if NETSTANDARD
            identifier = identifier.Substring( length );
#else
            identifier = identifier[length..];
#endif
        }

        group = date;
        return true;
    }

    private bool TryConsumeNumber( ref Text identifier, out int? number )
    {
        var length = 0;

        for ( var i = 0; i < identifier.Length; i++ )
        {
#if NETSTANDARD
            var ch = identifier[i];
#else
            ref readonly var ch = ref identifier[i];
#endif
            if ( !char.IsDigit( ch ) )
            {
                break;
            }

            length++;
        }

        if ( length == 0 )
        {
            number = default;
            return false;
        }

        if ( int.TryParse(
#if NETSTANDARD
            identifier.Substring( 0, length ),
#else
            identifier[..length],
#endif
            NumberStyles.Integer,
            FormatProvider,
            out var result ) )
        {
            if ( identifier.Length == length )
            {
#if NETSTANDARD
                identifier = string.Empty;
#else
                identifier = default;
#endif
            }
            else
            {
                if ( identifier[length] == '_' )
                {
                    length++;
                }
#if NETSTANDARD
                identifier = identifier.Substring( length );
#else
                identifier = identifier[length..];
#endif
            }

            number = result;
            return true;
        }

        number = default;
        return false;
    }
}