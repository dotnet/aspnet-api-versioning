// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.OData.Edm;

/// <summary>
/// Defines the behavior of an object that selects an <see cref="IEdmModel">EDM model</see>.
/// </summary>
public interface IEdmModelSelector
{
    /// <summary>
    /// Gets a read-only list of API versions that can be selected from.
    /// </summary>
    /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>.</value>
    IReadOnlyList<ApiVersion> ApiVersions { get; }

    /// <summary>
    /// Selects an EDM model using the given API version.
    /// </summary>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to select a model for.</param>
    /// <returns>The selected <see cref="IEdmModel">EDM model</see> or <c>null</c>.</returns>
    IEdmModel? SelectModel( ApiVersion? apiVersion );

    /// <summary>
    /// Selects an EDM model using the given service provider.
    /// </summary>
    /// <param name="serviceProvider">The current <see cref="IServiceProvider">service provider</see>.</param>
    /// <returns>The selected <see cref="IEdmModel">EDM model</see> or <c>null</c>.</returns>
    IEdmModel? SelectModel( IServiceProvider serviceProvider );

    /// <summary>
    /// Returns a value indicating whether the selector contains the specified API version.
    /// </summary>
    /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to evaluate.</param>
    /// <returns>True if the selector contains the <paramref name="apiVersion">API version</paramref>; otherwise, false.</returns>
    bool Contains( ApiVersion? apiVersion );
}