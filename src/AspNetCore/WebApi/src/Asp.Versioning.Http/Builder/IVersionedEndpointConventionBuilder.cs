// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;

/// <summary>
/// Defines the behavior of a versioned <see cref="IEndpointConventionBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public interface IVersionedEndpointConventionBuilder : IEndpointConventionBuilder, IMapToApiVersionConventionBuilder
{
    /// <summary>
    /// Gets or sets a value indicating whether requests report the API version compatibility information in responses.
    /// </summary>
    /// <value>True if API versions are reported; otherwise, false.</value>
    bool ReportApiVersions { get; set; }

    /// <summary>
    /// Builds and returns a new API version metadata.
    /// </summary>
    /// <param name="options">The configured <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    /// <returns>A new <see cref="ApiVersionMetadata">API version metadata</see>.</returns>
    ApiVersionMetadata Build( ApiVersioningOptions options );
}