// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http;
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
        ArgumentNullException.ThrowIfNull( endpoints );
        var create = endpoints.ServiceProvider.GetService<ApiVersionSetBuilderFactory>();
        return create is null ? new( name ) : create( name );
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
        ArgumentNullException.ThrowIfNull( builder );

        if ( builder.HasMetadata() )
        {
            throw new InvalidOperationException( SR.CannotNestVersionSet );
        }

        if ( !string.IsNullOrEmpty( name ) )
        {
            builder.Add( endpoint => endpoint.Metadata.Insert( 0, new TagsAttribute( name ) ) );
        }

        builder.Finally( EndpointBuilderFinalizer.FinalizeRoutes );

        return builder.NewVersionedEndpointRouteBuilder( builder, builder, name );
    }

    /// <summary>
    /// Creates a route group builder for defining all versioned endpoints in an API.
    /// </summary>
    /// <param name="builder">The extended <see cref="IEndpointRouteBuilder"/>.</param>
    /// <param name="name">The optional name associated with the builder.</param>
    /// <returns>A new <see cref="IVersionedEndpointRouteBuilder"/> instance.</returns>
    public static IVersionedEndpointRouteBuilder NewVersionedApi( this IEndpointRouteBuilder builder, string? name = default )
    {
        ArgumentNullException.ThrowIfNull( builder );

        if ( builder.IsNestedGroup() )
        {
            throw new InvalidOperationException( SR.CannotNestApiGroup );
        }

        var group = builder.MapGroup( string.Empty );
        IEndpointConventionBuilder convention = group;

        if ( !string.IsNullOrEmpty( name ) )
        {
            convention.Add( endpoint => endpoint.Metadata.Insert( 0, new TagsAttribute( name ) ) );
        }

        convention.Finally( EndpointBuilderFinalizer.FinalizeRoutes );

        return builder.NewVersionedEndpointRouteBuilder( group, group, name );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static IVersionedEndpointRouteBuilder NewVersionedEndpointRouteBuilder(
        this IEndpointRouteBuilder builder,
        IEndpointRouteBuilder routeBuilder,
        IEndpointConventionBuilder conventionBuilder,
        string? name )
    {
        var create = builder.ServiceProvider.GetService<VersionedEndpointRouteBuilderFactory>();
        var versionSet = builder.NewApiVersionSet( name );

        return create is null ?
               new VersionedEndpointRouteBuilder( routeBuilder, conventionBuilder, versionSet ) :
               create( routeBuilder, conventionBuilder, versionSet );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool HasMetadata( this IEndpointRouteBuilder builder ) =>
        builder.ServiceProvider.GetService<ApiVersionSetBuilder>() is not null;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static bool IsNestedGroup( this IEndpointRouteBuilder builder ) =>
        builder is RouteGroupBuilder || builder.HasMetadata();
}