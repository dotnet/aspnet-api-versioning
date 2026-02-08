// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

/// <summary>
/// Provides extension methods for <see cref="IApiVersionDescriptionProviderFactory"/>.
/// </summary>
[CLSCompliant( false )]
public static class IApiVersionDescriptionProviderFactoryExtensions
{
    /// <param name="factory">The extended <see cref="IApiVersionDescriptionProviderFactory"/>.</param>
    extension( IApiVersionDescriptionProviderFactory factory )
    {
        /// <summary>
        /// Creates and returns an API version description provider.
        /// </summary>
        /// <returns>A new <see cref="IApiVersionDescriptionProvider">API version description provider</see>.</returns>
        public IApiVersionDescriptionProvider Create()
        {
            ArgumentNullException.ThrowIfNull( factory );
            return factory.Create( new EmptyEndpointDataSource() );
        }
    }

    private sealed class EmptyEndpointDataSource : EndpointDataSource
    {
        public override IReadOnlyList<Endpoint> Endpoints { get; } = [];

        public override IChangeToken GetChangeToken() => new CancellationChangeToken( CancellationToken.None );

        public override IReadOnlyList<Endpoint> GetGroupedEndpoints( RouteGroupContext context ) => Endpoints;
    }
}