// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;

/// <summary>
/// Defines the behavior of an endpoint metadata builder.
/// </summary>
public interface IEndpointMetadataBuilder : IMapToApiVersionConventionBuilder
{
    /// <summary>
    /// Gets the provider used to resolve services.
    /// </summary>
    /// <value>The <see cref="IServiceProvider"/> used to resolve services.</value>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets or sets a value indicating whether requests report the API version compatibility information in responses.
    /// </summary>
    /// <value>True if API versions are reported; otherwise, false.</value>
    bool ReportApiVersions { get; set; }

    /// <summary>
    /// Builds and returns a new API version metadata.
    /// </summary>
    /// <returns>A new <see cref="ApiVersionMetadata">API version metadata</see>.</returns>
    ApiVersionMetadata Build();
}