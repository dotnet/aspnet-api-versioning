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
    private static readonly DateTime UnixEpoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
#endif

    /// <summary>
    /// Gets an API sunset policy from the HTTP response.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage">HTTP response</see> to read from.</param>
    /// <returns>A new <see cref="SunsetPolicy">sunset policy</see>.</returns>
    public static SunsetPolicy ReadSunsetPolicy( this HttpResponseMessage response )
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

        if ( headers.TryGetValues( Link, out values ) )
        {
            var baseUrl = response.RequestMessage?.RequestUri;
            Func<Uri, Uri> resolver = baseUrl is null ? url => url : url => new( baseUrl, url );

            foreach ( var value in values )
            {
                if ( LinkHeaderValue.TryParse( value, resolver, out var link ) &&
                     link.RelationType.Equals( "sunset", OrdinalIgnoreCase ) )
                {
                    policy.Links.Add( link );
                }
            }
        }

        return policy;
    }

    /// <summary>
    /// Formats the <paramref name="deprecationDate"/> as required for a Deprecation header.
    /// </summary>
    /// <param name="deprecationDate">The date when the api is deprecated.</param>
    /// <returns>A formatted string as required for a Deprecation header.</returns>
    public static string ToDeprecationHeaderValue( this DateTimeOffset deprecationDate )
    {
        var unixTimestamp = deprecationDate.ToUnixTimeSeconds();
        return unixTimestamp.ToString( "'@'0", CultureInfo.InvariantCulture );
    }

    /// <summary>
    /// Gets an API deprecation policy from the HTTP response.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage">HTTP response</see> to read from.</param>
    /// <returns>A new <see cref="DeprecationPolicy">deprecation policy</see>.</returns>
    public static DeprecationPolicy ReadDeprecationPolicy( this HttpResponseMessage response )
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
                if ( long.TryParse( value.Substring( 1 ), style, culture, out var unixTimestamp ) )
#else
                if ( long.TryParse( value.AsSpan()[1..], style, culture, out var unixTimestamp ) )
#endif
                {
                    DateTimeOffset parsed;
#if NETSTANDARD1_1
                    parsed = UnixEpoch + TimeSpan.FromSeconds( unixTimestamp );
#else
                    parsed = DateTimeOffset.FromUnixTimeSeconds( unixTimestamp );
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

        if ( headers.TryGetValues( Link, out values ) )
        {
            var baseUrl = response.RequestMessage?.RequestUri;
            Func<Uri, Uri> resolver = baseUrl is null ? url => url : url => new( baseUrl, url );

            foreach ( var value in values )
            {
                if ( LinkHeaderValue.TryParse( value, resolver, out var link ) &&
                     link.RelationType.Equals( "deprecation", OrdinalIgnoreCase ) )
                {
                    policy.Links.Add( link );
                }
            }
        }

        return policy;
    }

    /// <summary>
    /// Gets the OpenAPI document URLs from the HTTP response.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage">HTTP response</see> to read from.</param>
    /// <param name="parser">The optional <see cref="IApiVersionParser">parser</see> used to parse API versions.</param>
    /// <returns>A new <see cref="IReadOnlyDictionary{TKey, TValue}">read-only dictionary</see> of API version
    /// to URL mappings.</returns>
    public static IReadOnlyDictionary<ApiVersion, Uri> GetOpenApiDocumentUrls(
        this HttpResponseMessage response,
        IApiVersionParser? parser = default )
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