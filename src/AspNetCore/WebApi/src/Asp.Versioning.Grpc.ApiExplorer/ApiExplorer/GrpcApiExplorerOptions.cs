// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

/// <summary>
/// Represents the possible API versioning options for a gRPC API explorer.
/// </summary>
public class GrpcApiExplorerOptions
{
    /// <summary>
    /// Gets or sets information about the API version gRPC route parameter.
    /// </summary>
    /// <value>The API version gRPC <see cref="GrpcApiVersionRouteParameter">route parameter</see> information.</value>
    public GrpcApiVersionRouteParameter RouteParameter { get; protected set; } = new();
}