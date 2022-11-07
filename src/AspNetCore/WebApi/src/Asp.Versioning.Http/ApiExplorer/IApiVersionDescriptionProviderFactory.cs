// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Routing;

/// <summary>
/// Defines the behavior of a factory used to create a <see cref="IApiVersionDescriptionProvider"/>.
/// </summary>
[CLSCompliant( false )]
public interface IApiVersionDescriptionProviderFactory
{
    /// <summary>
    /// Creates and returns an API version description provider.
    /// </summary>
    /// <param name="endpointDataSource">The <see cref="EndpointDataSource">endpoint data
    /// source</see> used by the provider.</param>
    /// <returns>A new <see cref="IApiVersionDescriptionProvider">API version description provider</see>.</returns>
    IApiVersionDescriptionProvider Create( EndpointDataSource endpointDataSource );
}