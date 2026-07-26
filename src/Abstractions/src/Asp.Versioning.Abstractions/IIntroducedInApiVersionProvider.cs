// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of an <see cref="ApiVersion">API version</see> provider that describes when an API was introduced.
/// </summary>
public interface IIntroducedInApiVersionProvider : IApiVersionProvider
{
    /// <summary>
    /// Gets the HTTP status code returned when the requested API version is earlier than the introduced API version.
    /// </summary>
    /// <value>The HTTP status code.</value>
    int StatusCode { get; }
}