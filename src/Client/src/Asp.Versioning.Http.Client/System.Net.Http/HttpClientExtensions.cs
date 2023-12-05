// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Net.Http;

using Asp.Versioning;
using Asp.Versioning.Http;
using static System.Net.HttpStatusCode;
#if NETSTANDARD
using ArgumentNullException = Backport.ArgumentNullException;
#endif

/// <summary>
/// Provides extension methods for <see cref="HttpClient"/>.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Gets the API information for the specified request URL.
    /// </summary>
    /// <param name="client">The extended <see cref="HttpClient">HTTP client</see>.</param>
    /// <param name="requestUrl">The URL to get the API information from.</param>
    /// <param name="parser">The optional <see cref="IApiVersionParser">parser</see> used
    /// to process retrieved API versions.</param>
    /// <param name="enumerable">The optional <see cref="ApiVersionHeaderEnumerable">enumerable</see>
    /// used to enumerate retrieved API versions.</param>
    /// <param name="cancellationToken">The token that can be used to cancel the operation.</param>
    /// <returns>A task containing the retrieved <see cref="ApiInformation">API information</see>.</returns>
    /// <remarks>API information is retrieved by sending an OPTIONS request to the specified URL.
    /// If the resource does not exist or OPTIONS is not allowed, then <see cref="ApiInformation.None"/>
    /// will be returned.</remarks>
    public static Task<ApiInformation> GetApiInformationAsync(
        this HttpClient client,
        string requestUrl,
        IApiVersionParser? parser = default,
        ApiVersionHeaderEnumerable? enumerable = default,
        CancellationToken cancellationToken = default ) =>
        client.GetApiInformationAsync(
            new Uri( requestUrl, UriKind.RelativeOrAbsolute ),
            parser,
            enumerable,
            cancellationToken );

    /// <summary>
    /// Gets the API information for the specified request URL.
    /// </summary>
    /// <param name="client">The extended <see cref="HttpClient">HTTP client</see>.</param>
    /// <param name="requestUrl">The URL to get the API information from.</param>
    /// <param name="parser">The optional <see cref="IApiVersionParser">parser</see> used
    /// to process retrieved API versions.</param>
    /// <param name="enumerable">The optional <see cref="ApiVersionHeaderEnumerable">enumerable</see>
    /// used to enumerate retrieved API versions.</param>
    /// <param name="cancellationToken">The token that can be used to cancel the operation.</param>
    /// <returns>A task containing the retrieved <see cref="ApiInformation">API information</see>.</returns>
    /// <remarks>API information is retrieved by sending an OPTIONS request to the specified URL.
    /// If the resource does not exist or OPTIONS is not allowed, then <see cref="ApiInformation.None"/>
    /// will be returned.</remarks>
    public static async Task<ApiInformation> GetApiInformationAsync(
        this HttpClient client,
        Uri requestUrl,
        IApiVersionParser? parser = default,
        ApiVersionHeaderEnumerable? enumerable = default,
        CancellationToken cancellationToken = default )
    {
        ArgumentNullException.ThrowIfNull( client );

        using var request = new HttpRequestMessage( HttpMethod.Options, requestUrl );
        var response = await client.SendAsync( request, cancellationToken ).ConfigureAwait( false );

        response.RequestMessage ??= request;

        switch ( response.StatusCode )
        {
            case NotFound:
            case MethodNotAllowed:
                return ApiInformation.None;
        }

        parser ??= ApiVersionParser.Default;
        enumerable ??= new();
        var versions = new SortedSet<ApiVersion>( enumerable.Supported( response, parser ) );
        var supported = versions.ToArray();

        versions.Clear();

        foreach ( var version in enumerable.Deprecated( response, parser ) )
        {
            versions.Add( version );
        }

        if ( supported.Length == 0 && versions.Count == 0 )
        {
            return ApiInformation.None;
        }

        var deprecated = versions.ToArray();
        var sunsetPolicy = response.ReadSunsetPolicy();
        var urls = response.GetOpenApiDocumentUrls( parser );

        return new( supported, deprecated, sunsetPolicy, urls );
    }
}