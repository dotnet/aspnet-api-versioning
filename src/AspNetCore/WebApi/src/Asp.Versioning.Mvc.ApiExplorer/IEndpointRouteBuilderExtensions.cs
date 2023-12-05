// Copyright (c) .NET Foundation and contributors. All rights reserved.

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
    /// <summary>
    /// Returns a read-only list of API version descriptions.
    /// </summary>
    /// <param name="endpoints">The <see cref="IEndpointRouteBuilder">endpoints</see> to build the
    /// API version descriptions from.</param>
    /// <returns>A new <see cref="IReadOnlyList{T}">read-only list</see> of<see cref="ApiVersionDescription">API version descriptions</see>.</returns>
    public static IReadOnlyList<ApiVersionDescription> DescribeApiVersions( this IEndpointRouteBuilder endpoints )
    {
        ArgumentNullException.ThrowIfNull( endpoints );

        var services = endpoints.ServiceProvider;
        var factory = services.GetRequiredService<IApiVersionDescriptionProviderFactory>();
        using var source = new CompositeEndpointDataSource( endpoints.DataSources );
        var provider = factory.Create( source );

        return provider.ApiVersionDescriptions;
    }
}