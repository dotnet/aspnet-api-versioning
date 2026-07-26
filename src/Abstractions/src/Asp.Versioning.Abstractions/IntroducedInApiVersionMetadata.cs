// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents endpoint metadata that describes when an API action was introduced.
/// </summary>
public sealed class IntroducedInApiVersionMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IntroducedInApiVersionMetadata"/> class.
    /// </summary>
    /// <param name="introducedIn">The <see cref="ApiVersion">API version</see> in which the API action was introduced.</param>
    /// <param name="statusCode">The HTTP status code returned for earlier controller-declared API versions.</param>
    public IntroducedInApiVersionMetadata( ApiVersion introducedIn, int statusCode )
    {
        IntroducedIn = introducedIn;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets the API version in which the API action was introduced.
    /// </summary>
    /// <value>The introduced <see cref="ApiVersion">API version</see>.</value>
    public ApiVersion IntroducedIn { get; }

    /// <summary>
    /// Gets the HTTP status code returned for earlier controller-declared API versions.
    /// </summary>
    /// <value>The HTTP status code.</value>
    public int StatusCode { get; }
}