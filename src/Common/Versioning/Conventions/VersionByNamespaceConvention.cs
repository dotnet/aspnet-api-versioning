#pragma warning disable IDE0079
#pragma warning disable SA1121
#pragma warning disable SA1114 // Parameter list should follow declaration

#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
#if NETFRAMEWORK
    using Text = System.String;
#else
    using Text = System.ReadOnlySpan<char>;
#endif

    /// <summary>
    /// Represents a convention which applies an API to a controller by its defined namespace.
    /// </summary>
    public partial class VersionByNamespaceConvention
    {
        const string CompactDateFormat = "yyyyMMdd";
        const string ReadableDateFormat = "yyyy_MM_dd";

        static ApiVersion? GetApiVersion( string @namespace )
        {
            var versions = Parse( @namespace );

            return versions.Count switch
            {
                0 => default,
                1 => versions[0],
                _ => throw new InvalidOperationException( SR.MultipleApiVersionsInferredFromNamespaces.FormatInvariant( @namespace ) ),
            };
        }

        static IReadOnlyList<ApiVersion> Parse( string @namespace )
        {
#if NETFRAMEWORK
            var text = @namespace;
#else
            var text = @namespace.AsSpan();
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
#if NETFRAMEWORK
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
                        versions = new() { version, result };
                    }
                    else
                    {
                        versions.Add( result );
                    }
                }

                start = end + 1;
            }

            if ( TryParse(
#if NETFRAMEWORK
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
#if NETFRAMEWORK
                return new ApiVersion[0];
#else
                return Array.Empty<ApiVersion>();
#endif
            }
            else if ( versions is null )
            {
                return new[] { version };
            }

            return versions;
        }

        static bool TryParse( Text identifier, out ApiVersion? apiVersion )
        {
#if NETFRAMEWORK
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

#if NETFRAMEWORK
            identifier = identifier.Substring( 1 );
#else
            identifier = identifier[1..];
#endif
            var group = default( DateTime? );

            if ( identifier.Length >= 8 )
            {
                if ( !TryConsumeGroup( ref identifier, CompactDateFormat, length: 8, out group ) )
                {
                    apiVersion = default;
                    return false;
                }

                if ( group is null && identifier.Length >= 10 )
                {
                    if ( !TryConsumeGroup( ref identifier, ReadableDateFormat, length: 10, out group ) )
                    {
                        apiVersion = default;
                        return false;
                    }
                }
            }

            int? major;
            int? minor;
            string? status;

#if NETFRAMEWORK
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
                    if ( !TryConsumeNumber( ref identifier, out minor ) )
                    {
                        minor = 0;
                    }

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

            if ( !string.IsNullOrEmpty( status ) && !ApiVersion.IsValidStatus( status ) )
            {
                apiVersion = default;
                return false;
            }

            apiVersion = new( group, major, minor, status );
            return true;
        }

        static bool IsDateLike( Text value )
        {
            if ( value.Length == 8 )
            {
                for ( var i = 0; i < 8; i++ )
                {
#if NETFRAMEWORK
                    var ch = value[i];
#else
                    ref readonly var ch = ref value[i];
#endif
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
#if NETFRAMEWORK
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

        static bool TryConsumeGroup( ref Text identifier, string format, int length, out DateTime? group )
        {
#if NETFRAMEWORK
            var segment = identifier.Substring( 0, length );
#else
            var segment = identifier[..length];
#endif

            if ( !DateTime.TryParseExact( segment, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date ) )
            {
                group = default;
                return !IsDateLike( segment );
            }

            if ( identifier.Length == length )
            {
#if NETFRAMEWORK
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
#if NETFRAMEWORK
                identifier = identifier.Substring( length );
#else
                identifier = identifier[length..];
#endif
            }

            group = date;
            return true;
        }

        static bool TryConsumeNumber( ref Text identifier, out int? number )
        {
            var length = 0;

            for ( var i = 0; i < identifier.Length; i++ )
            {
#if NETFRAMEWORK
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
#if NETFRAMEWORK
                identifier.Substring( 0, length ),
#else
                identifier[..length],
#endif
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var result ) )
            {
                if ( identifier.Length == length )
                {
#if NETFRAMEWORK
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
#if NETFRAMEWORK
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
}