// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Creates and returns a new versioned endpoint route builder.
/// </summary>
/// <param name="routeBuilder">The inner <see cref="IEndpointRouteBuilder"/> the new instance decorates.</param>
/// <param name="conventionBuilder">The inner <see cref="IEndpointConventionBuilder"/> the new instance decorates.</param>
/// <param name="apiVersionSetBuilder">The associated <see cref="ApiVersionSetBuilder">API version set builder</see>.</param>
/// <returns>A new <see cref="IVersionedEndpointRouteBuilder"/> instance.</returns>
[CLSCompliant( false )]
public delegate IVersionedEndpointRouteBuilder VersionedEndpointRouteBuilderFactory(
    IEndpointRouteBuilder routeBuilder,
    IEndpointConventionBuilder conventionBuilder,
    ApiVersionSetBuilder apiVersionSetBuilder );