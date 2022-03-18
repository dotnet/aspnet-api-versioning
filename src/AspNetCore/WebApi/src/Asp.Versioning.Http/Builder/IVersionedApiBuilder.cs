// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Defines the behavior of a versioned API builder.
/// </summary>
[CLSCompliant( false )]
public interface IVersionedApiBuilder : IDeclareApiVersionConventionBuilder
{
    /// <summary>
    /// Gets the provider used to resolve services.
    /// </summary>
    /// <value>The <see cref="IServiceProvider"/> used to resolve services.</value>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets a collection of endpoint data sources.
    /// </summary>
    /// <value>A <see cref="ICollection{T}">collection</see> of <see cref="EndpointDataSource">endpoint data sources</see>.</value>
    ICollection<EndpointDataSource> DataSources { get; }

    /// <summary>
    /// Gets the name of the API.
    /// </summary>
    /// <value>The API name, if specified.</value>
    string? Name { get; }

    /// <summary>
    /// Gets or sets a value indicating whether requests report the API version compatibility information in responses.
    /// </summary>
    /// <value>True if API versions are reported; otherwise, false.</value>
    bool ReportApiVersions { get; set; }

    /// <summary>
    /// Configures the endpoint mappings for a versioned API.
    /// </summary>
    /// <param name="api">The <see cref="IVersionedEndpointBuilder">builder</see> used to map endpoints.</param>
    void HasMapping( Action<IVersionedEndpointBuilder> api );

    /// <summary>
    /// Builds and returns a new API version model.
    /// </summary>
    /// <returns>A new <see cref="ApiVersionModel">API version model</see>.</returns>
    ApiVersionModel Build();
}