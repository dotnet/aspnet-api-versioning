// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;

/// <summary>
/// Represents the versioned OpenAPI configuration options.
/// </summary>
public class VersionedOpenApiOptions
{
    /// <summary>
    /// Gets the associated API version description.
    /// </summary>
    /// <value>The associated <see cref="ApiVersionDescription">associated API version description</see>.</value>
    public required ApiVersionDescription Description { get; init; }

    /// <summary>
    /// Gets the OpenAPI options.
    /// </summary>
    /// <value>The associated <see cref="OpenApiOptions">OpenAPI options</see>.</value>
    [CLSCompliant( false )]
    public required OpenApiOptions Document { get; init; }

    /// <summary>
    /// Gets the OpenAPI document description configuration options.
    /// </summary>
    /// <value>These <see cref="OpenApiDocumentDescriptionOptions">options</see> provide additional configuration
    /// for indicating which additional information should be included in an OpenAPI document description such
    /// as the deprecation and sunset policies.</value>
    public required OpenApiDocumentDescriptionOptions DocumentDescription { get; init; }
}