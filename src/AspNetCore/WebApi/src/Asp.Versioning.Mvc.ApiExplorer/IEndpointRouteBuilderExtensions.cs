// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static class IEndpointRouteBuilderExtensions
{
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder">endpoints</see> to build the API version
    /// descriptions from.</param>
    extension( IEndpointRouteBuilder endpoints )
    {
        /// <summary>
        /// Returns a read-only list of API version descriptions.
        /// </summary>
        /// <returns>A new <see cref="IReadOnlyList{T}">read-only list</see> of<see cref="ApiVersionDescription">API
        /// version descriptions</see>.</returns>
        public IReadOnlyList<ApiVersionDescription> DescribeApiVersions()
        {
            ArgumentNullException.ThrowIfNull( endpoints );

            var services = endpoints.ServiceProvider;
            var factory = services.GetRequiredService<IApiVersionDescriptionProviderFactory>();
            using var source = new CompositeEndpointDataSource( endpoints.DataSources );
            var provider = factory.Create( source );

            return provider.ApiVersionDescriptions;
        }
    }
}