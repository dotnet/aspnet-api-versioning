// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Creates and returns a new API version set builder for the specified endpoints.
    /// </summary>
    /// <param name="endpoints">The extended <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="name">The optional name of the API.</param>
    /// <returns>A new <see cref="ApiVersionSetBuilder">API version set builder</see>.</returns>
    public static ApiVersionSetBuilder NewApiVersionSet( this IEndpointRouteBuilder endpoints, string? name = default )
    {
        if ( endpoints == null )
        {
            throw new ArgumentNullException( nameof( endpoints ) );
        }

        var factory = endpoints.ServiceProvider.GetRequiredService<IApiVersionSetBuilderFactory>();

        return factory.Create( name );
    }
}