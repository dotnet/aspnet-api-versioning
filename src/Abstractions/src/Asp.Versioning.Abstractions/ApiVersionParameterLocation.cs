// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Represents the supported API version parameter description locations.
/// </summary>
public enum ApiVersionParameterLocation
{
    /// <summary>
    /// Indicates the API version is expressed as a HTTP query string parameter.
    /// </summary>
    Query,

    /// <summary>
    /// Indicates the API version is expressed as a HTTP header.
    /// </summary>
    Header,

    /// <summary>
    /// Indicates the API version is expressed in a URL path segment.
    /// </summary>
    Path,

    /// <summary>
    /// Indicates the API version is expressed as a media type parameter.
    /// </summary>
    MediaTypeParameter,
}