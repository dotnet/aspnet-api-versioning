// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IEndpointRouteBuilderExtensions
{
    /// <param name="builder">The extended <see cref="IEndpointRouteBuilder"/>.</param>
    extension( IEndpointRouteBuilder builder )
    {
        /// <summary>
        /// Creates and returns a new API version set builder for the specified endpoints.
        /// </summary>
        /// <param name="name">The optional name of the API.</param>
        /// <returns>A new <see cref="ApiVersionSetBuilder">API version set builder</see>.</returns>
        public ApiVersionSetBuilder NewApiVersionSet( string? name = default )
        {
            ArgumentNullException.ThrowIfNull( builder );
            var create = builder.ServiceProvider.GetService<ApiVersionSetBuilderFactory>();
            return create is null ? new( name ) : create( name );
        }

        /// <summary>
        /// Creates a route group builder for defining all versioned endpoints in an API.
        /// </summary>
        /// <param name="name">The optional name associated with the builder.</param>
        /// <returns>A new <see cref="IVersionedEndpointRouteBuilder"/> instance.</returns>
        public IVersionedEndpointRouteBuilder NewVersionedApi( string? name = default )
        {
            ArgumentNullException.ThrowIfNull( builder );

            if ( builder.IsNestedGroup )
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

        private IVersionedEndpointRouteBuilder NewVersionedEndpointRouteBuilder(
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

        private bool HasMetadata => builder.ServiceProvider.GetService<ApiVersionSetBuilder>() is not null;

        private bool IsNestedGroup => builder is RouteGroupBuilder || builder.HasMetadata;
    }

    /// <typeparam name="TBuilder">The type of builder.</typeparam>
    /// <param name="builder">The extended builder.</param>
    extension<TBuilder>( TBuilder builder ) where TBuilder : notnull, IEndpointRouteBuilder, IEndpointConventionBuilder
    {
        /// <summary>
        /// Applies the specified API version set to the endpoint group.
        /// </summary>
        /// <param name="name">The optional name associated with the builder.</param>
        /// <returns>A new <see cref="IVersionedEndpointRouteBuilder"/> instance.</returns>
        public IVersionedEndpointRouteBuilder WithApiVersionSet( string? name = default )
        {
            ArgumentNullException.ThrowIfNull( builder );

            if ( builder.HasMetadata )
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
    }
}