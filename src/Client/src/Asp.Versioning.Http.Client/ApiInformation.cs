// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

/// <summary>
/// Represents the information for an API.
/// </summary>
public class ApiInformation
{
    private static ApiInformation? none;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiInformation"/> class.
    /// </summary>
    /// <param name="supportedVersions">The supported read-only list of API versions.</param>
    /// <param name="deprecatedVersions">The deprecated read-only list of API versions.</param>
    /// <param name="sunsetPolicy">The API sunset policy.</param>
    /// <param name="openApiDocumentUrls">The read-only mapping of API version to OpenAPI document URLs.</param>
    public ApiInformation(
        IReadOnlyList<ApiVersion> supportedVersions,
        IReadOnlyList<ApiVersion> deprecatedVersions,
        SunsetPolicy sunsetPolicy,
        IReadOnlyDictionary<ApiVersion, Uri> openApiDocumentUrls )
    {
        SupportedApiVersions = supportedVersions ?? throw new System.ArgumentNullException( nameof( supportedVersions ) );
        DeprecatedApiVersions = deprecatedVersions ?? throw new System.ArgumentNullException( nameof( deprecatedVersions ) );
        SunsetPolicy = sunsetPolicy ?? throw new System.ArgumentNullException( nameof( sunsetPolicy ) );
        OpenApiDocumentUrls = openApiDocumentUrls ?? throw new System.ArgumentNullException( nameof( openApiDocumentUrls ) );
    }

    private ApiInformation()
    {
        SupportedApiVersions = Array.Empty<ApiVersion>();
        DeprecatedApiVersions = Array.Empty<ApiVersion>();
        SunsetPolicy = new();
        OpenApiDocumentUrls = new Dictionary<ApiVersion, Uri>( capacity: 0 );
    }

    /// <summary>
    /// Gets an instance that represents no API information.
    /// </summary>
    /// <value>An instance that represents no API information.</value>
    public static ApiInformation None => none ??= new();

    /// <summary>
    /// Gets the versions supported by the API.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of support <see cref="ApiVersion">API versions</see>.</value>
    public IReadOnlyList<ApiVersion> SupportedApiVersions { get; }

    /// <summary>
    /// Gets the versions deprecated by the API.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of deprecated <see cref="ApiVersion">API versions</see>.</value>
    /// <remarks>A deprecated API version does not mean it is not supported. A deprecated API version is typically advertised six
    /// months or more before it becomes unsupported.</remarks>
    public IReadOnlyList<ApiVersion> DeprecatedApiVersions { get; }

    /// <summary>
    /// Gets the API sunset policy.
    /// </summary>
    /// <value>The <see cref="SunsetPolicy">sunset policy</see> for the API.</value>
    public SunsetPolicy SunsetPolicy { get; }

    /// <summary>
    /// Gets the OpenAPI document URLs for each version.
    /// </summary>
    /// <value>A <see cref="IReadOnlyDictionary{TKey, TValue}">read-only dictionary</see> of <see cref="ApiVersion">API version</see>
    /// to <see cref="Uri">URL</see> mappings for each OpenAPI document.</value>
    /// <remarks>If the API provides a single OpenAPI document that does not map to a specific <see cref="ApiVersion">API version</see>,
    /// the <see cref="Uri">URL</see> will be mapped to <see cref="ApiVersion.Neutral"/>.</remarks>
    public IReadOnlyDictionary<ApiVersion, Uri> OpenApiDocumentUrls { get; }
}