﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

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

    /// <summary>
    /// Applies the specified API version set to the endpoint group.
    /// </summary>
    /// <typeparam name="TBuilder">The type of builder.</typeparam>
    /// <param name="builder">The extended builder.</param>
    /// <param name="name">The optional name associated with the builder.</param>
    /// <returns>A new <see cref="IVersionedEndpointRouteBuilder"/> instance.</returns>
    public static IVersionedEndpointRouteBuilder WithApiVersionSet<TBuilder>( this TBuilder builder, string? name = default )
        where TBuilder : notnull, IEndpointRouteBuilder, IEndpointConventionBuilder
    {
        if ( builder is null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        if ( builder.HasMetadata() )
        {
            throw new InvalidOperationException( SR.CannotNestVersionSet );
        }

        var factory = builder.ServiceProvider.GetRequiredService<IApiVersionSetBuilderFactory>();

        builder.Finally( EndpointBuilderFinalizer.FinalizeRoutes );

        return new VersionedEndpointRouteBuilder( builder, builder, factory.Create( name ) );
    }

    /// <summary>
    /// Creates a route group builder for defining all versioned endpoints in an API.
    /// </summary>
    /// <param name="builder">The extended <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="name">The optional name associated with the builder.</param>
    /// <returns>A new <see cref="IVersionedEndpointRouteBuilder"/> instance.</returns>
    public static IVersionedEndpointRouteBuilder MapApiGroup( this IEndpointRouteBuilder builder, string? name = default )
    {
        if ( builder is null )
        {
            throw new ArgumentNullException( nameof( builder ) );
        }

        if ( builder.IsNestedGroup() )
        {
            throw new InvalidOperationException( SR.CannotNestApiGroup );
        }

        var group = builder.MapGroup( string.Empty );
        IEndpointConventionBuilder convention = group;
        var factory = builder.ServiceProvider.GetRequiredService<IApiVersionSetBuilderFactory>();

        convention.Finally( EndpointBuilderFinalizer.FinalizeRoutes );

        return new VersionedEndpointRouteBuilder( group, group, factory.Create( name ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool HasMetadata( this IEndpointRouteBuilder builder ) =>
        builder.ServiceProvider.GetService<ApiVersionSetBuilder>() is not null;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool IsNestedGroup( this IEndpointRouteBuilder builder ) =>
        builder is RouteGroupBuilder || builder.HasMetadata();
}