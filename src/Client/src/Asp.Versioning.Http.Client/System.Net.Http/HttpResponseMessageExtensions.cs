// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Net.Http;

using Asp.Versioning;
using System.Globalization;
using static System.StringComparison;
#if NETSTANDARD
using ArgumentNullException = Backport.ArgumentNullException;
#endif

/// <summary>
/// Provides extension methods for <see cref="HttpResponseMessage"/>.
/// </summary>
public static class HttpResponseMessageExtensions
{
    private const string Sunset = nameof( Sunset );
    private const string Deprecation = nameof( Deprecation );
    private const string Link = nameof( Link );
#if NETSTANDARD1_1
    private static readonly DateTime UnixEpoch = new( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
#endif

    extension( HttpResponseMessage response )
    {
        /// <summary>
        /// Gets an API sunset policy from the HTTP response.
        /// </summary>
        /// <returns>A new <see cref="SunsetPolicy">sunset policy</see>.</returns>
        public SunsetPolicy SunsetPolicy
        {
            get
            {
                ArgumentNullException.ThrowIfNull( response );

                var headers = response.Headers;
                var date = default( DateTimeOffset );
                SunsetPolicy policy;

                if ( headers.TryGetValues( Sunset, out var values ) )
                {
                    var culture = CultureInfo.CurrentCulture;
                    var style = DateTimeStyles.RoundtripKind;

                    foreach ( var value in values )
                    {
                        if ( DateTimeOffset.TryParse( value, culture, style, out var result ) &&
                             ( date == default || date < result ) )
                        {
                            date = result;
                        }
                    }

                    policy = date == default ? new() : new( date );
                }
                else
                {
                    policy = new();
                }

                response.AddLinks( policy.Links, "sunset" );

                return policy;
            }
        }

        /// <summary>
        /// Gets an API deprecation policy from the HTTP response.
        /// </summary>
        /// <returns>A new <see cref="DeprecationPolicy">deprecation policy</see>.</returns>
        public DeprecationPolicy DeprecationPolicy
        {
            get
            {
                ArgumentNullException.ThrowIfNull( response );

                var headers = response.Headers;
                var date = default( DateTimeOffset );
                DeprecationPolicy policy;

                if ( headers.TryGetValues( Deprecation, out var values ) )
                {
                    var culture = CultureInfo.InvariantCulture;
                    var style = NumberStyles.Integer;

                    foreach ( var value in values )
                    {
                        if ( value.Length < 2 || value[0] != '@' )
                        {
                            continue;
                        }

#if NETSTANDARD
                        if ( long.TryParse( value.Substring( 1 ), style, culture, out var seconds ) )
#else
                        if ( long.TryParse( value.AsSpan()[1..], style, culture, out var seconds ) )
#endif
                        {
                            DateTimeOffset parsed;
#if NETSTANDARD1_1
                            parsed = UnixEpoch + TimeSpan.FromSeconds( seconds );
#else
                            parsed = DateTimeOffset.FromUnixTimeSeconds( seconds );
#endif
                            if ( date == default || date > parsed )
                            {
                                date = parsed;
                            }
                        }
                    }

                    policy = date == default ? new() : new( date );
                }
                else
                {
                    policy = new();
                }

                response.AddLinks( policy.Links, "deprecation" );

                return policy;
            }
        }

        /// <summary>
        /// Gets the OpenAPI document URLs from the HTTP response.
        /// </summary>
        /// <param name="parser">The optional <see cref="IApiVersionParser">parser</see> used to parse API versions.</param>
        /// <returns>A new <see cref="IReadOnlyDictionary{TKey, TValue}">read-only dictionary</see> of API version
        /// to URL mappings.</returns>
        public IReadOnlyDictionary<ApiVersion, Uri> GetOpenApiDocumentUrls( IApiVersionParser? parser = default )
        {
            ArgumentNullException.ThrowIfNull( response );

            var urls = default( Dictionary<ApiVersion, Uri> );

            if ( response.Headers.TryGetValues( Link, out var values ) )
            {
                var baseUrl = response.RequestMessage?.RequestUri;
                Func<Uri, Uri> resolver = baseUrl is null ? url => url : url => new( baseUrl, url );

                foreach ( var value in values )
                {
                    if ( !LinkHeaderValue.TryParse( value, resolver, out var link ) ||
                         ( !link.RelationType.Equals( "openapi", OrdinalIgnoreCase ) &&
                           !link.RelationType.Equals( "swagger", OrdinalIgnoreCase ) ) )
                    {
                        continue;
                    }

                    var key = GetApiVersionExtension( link, ref parser );
                    urls ??= [];
                    urls[key] = link.LinkTarget;
                }

                urls ??= [];
            }
            else
            {
                urls = [];
            }

            return urls;
        }

        private void AddLinks( IList<LinkHeaderValue> links, string relationType )
        {
            if ( !response.Headers.TryGetValues( Link, out var values ) )
            {
                return;
            }

            var baseUrl = response.RequestMessage?.RequestUri;
            Func<Uri, Uri> resolver = baseUrl is null ? url => url : url => new( baseUrl, url );

            foreach ( var value in values )
            {
                if ( LinkHeaderValue.TryParse( value, resolver, out var link ) &&
                     link.RelationType.Equals( relationType, OrdinalIgnoreCase ) )
                {
                    links.Add( link );
                }
            }
        }
    }

    private static ApiVersion GetApiVersionExtension( LinkHeaderValue link, ref IApiVersionParser? parser )
    {
        if ( link.Extensions.TryGetValue( "api-version", out var extension ) )
        {
            parser ??= ApiVersionParser.Default;
#if NETSTANDARD
            var value = extension.ToString();
#else
            var value = extension.AsSpan();
#endif
            if ( parser.TryParse( value, out var version ) )
            {
                return version!;
            }
        }

        return ApiVersion.Neutral;
    }
}