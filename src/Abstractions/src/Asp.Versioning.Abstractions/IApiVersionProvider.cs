// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

/// <summary>
/// Defines the behavior of an <see cref="ApiVersion">API version</see> provider.
/// </summary>
public interface IApiVersionProvider
{
    /// <summary>
    /// Gets the options associated with the provided API versions.
    /// </summary>
    ApiVersionProviderOptions Options { get; }

    /// <summary>
    /// Gets the defined API versions defined.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>.</value>
    IReadOnlyList<ApiVersion> Versions { get; }
}