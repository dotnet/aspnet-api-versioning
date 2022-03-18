// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

/// <summary>
/// Defines the behavior of a provider that collects OData-specific API versions.
/// </summary>
public interface IODataApiVersionCollectionProvider
{
    /// <summary>
    /// Gets or sets a list of all OData API versions.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of
    /// <see cref="ApiVersion">API versions</see>.</value>
    IReadOnlyList<ApiVersion> ApiVersions { get; set; }
}