// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

/// <summary>
/// Defines the behavior of a provider that discovers and describes API version information within an application.
/// </summary>
public interface IApiVersionDescriptionProvider
{
    /// <summary>
    /// Gets a read-only list of discovered API version descriptions.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersionDescription">API version descriptions</see>.</value>
    IReadOnlyList<ApiVersionDescription> ApiVersionDescriptions { get; }
}