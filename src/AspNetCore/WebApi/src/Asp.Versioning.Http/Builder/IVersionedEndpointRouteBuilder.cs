// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Defines the behavior of a versioned <see cref="IEndpointRouteBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public interface IVersionedEndpointRouteBuilder :
    IEndpointRouteBuilder,
    IEndpointConventionBuilder
{
}