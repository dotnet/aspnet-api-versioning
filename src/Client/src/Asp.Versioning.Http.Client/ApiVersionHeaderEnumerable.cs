// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

/// <summary>
/// Represents the enumerable object used to create API version enumerators.
/// </summary>
public sealed class ApiVersionHeaderEnumerable
{
    private const string ApiSupportedVersions = "api-supported-versions";
    private const string ApiDeprecatedVersions = "api-deprecated-versions";
    private readonly string apiSupportedVersionsName;
    private readonly string apiDeprecatedVersionsName;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionHeaderEnumerable"/> class.
    /// </summary>
    /// <param name="supportedHeaderName">The HTTP header name used for supported API versions.
    /// The default value is "api-supported-versions".</param>
    /// <param name="deprecatedHeaderName">THe HTTP header name used for deprecated API versions.
    /// The default value is "api-deprecated-versions".</param>
    public ApiVersionHeaderEnumerable(
        string supportedHeaderName = ApiSupportedVersions,
        string deprecatedHeaderName = ApiDeprecatedVersions )
    {
        ArgumentException.ThrowIfNullOrEmpty( supportedHeaderName );
        ArgumentException.ThrowIfNullOrEmpty( deprecatedHeaderName );

        apiSupportedVersionsName = supportedHeaderName;
        apiDeprecatedVersionsName = deprecatedHeaderName;
    }

    /// <summary>
    /// Creates and returns an enumerator for supported API versions.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage">HTTP response</see> to evaluate.</param>
    /// <param name="parser">The optional <see cref="IApiVersionParser">API version parser</see>.</param>
    /// <returns>A new <see cref="ApiVersionEnumerator"/>.</returns>
    public ApiVersionEnumerator Supported(
        HttpResponseMessage response,
        IApiVersionParser? parser = default ) =>
        new( response, apiSupportedVersionsName, parser );

    /// <summary>
    /// Creates and returns an enumerator for deprecated API versions.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage">HTTP response</see> to evaluate.</param>
    /// <param name="parser">The optional <see cref="IApiVersionParser">API version parser</see>.</param>
    /// <returns>A new <see cref="ApiVersionEnumerator"/>.</returns>
    public ApiVersionEnumerator Deprecated(
        HttpResponseMessage response,
        IApiVersionParser? parser = default ) =>
        new( response, apiDeprecatedVersionsName, parser );
}