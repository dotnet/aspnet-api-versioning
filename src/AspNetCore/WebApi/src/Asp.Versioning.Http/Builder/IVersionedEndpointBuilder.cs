// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing.Patterns;

/// <summary>
/// Defines the behavior of a builder for versioned endpoints.
/// </summary>
[CLSCompliant( false )]
public interface IVersionedEndpointBuilder
{
    /// <summary>
    /// Adds an endpoint to the collection that matches HTTP requests for the specified pattern.
    /// </summary>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="requestDelegate">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="IEndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    IEndpointMetadataBuilder Map( RoutePattern pattern, RequestDelegate requestDelegate );

    /// <summary>
    /// Adds an endpoint to the collection that matches HTTP requests for the specified HTTP methods and pattern.
    /// </summary>
    /// <param name="pattern">The route pattern.</param>
    /// <param name="httpMethods">HTTP methods that the endpoint will match.</param>
    /// <param name="handler">The delegate executed when the endpoint is matched.</param>
    /// <returns>A <see cref="IEndpointMetadataBuilder"/> that can be used to further customize the endpoint.</returns>
    IEndpointMetadataBuilder MapMethods( string pattern, IEnumerable<string> httpMethods, Delegate handler );
}