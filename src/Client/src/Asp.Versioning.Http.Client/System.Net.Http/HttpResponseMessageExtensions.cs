// Copyright (c) .NET Foundation and contributors. All rights reserved.

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
    private const string Link = nameof( Link );

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

            urls ??= new( capacity: 0 );
        }
        else
        {
            urls = new( capacity: 0 );
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