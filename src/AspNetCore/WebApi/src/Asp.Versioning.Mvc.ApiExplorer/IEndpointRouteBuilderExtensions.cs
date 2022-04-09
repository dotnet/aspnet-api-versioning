// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Builder;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
        if ( endpoints == null )
        {
            throw new ArgumentNullException( nameof( endpoints ) );
        }

        // this should be produced by IApiVersionDescriptionProvider via di; however, for minimal apis, the
        // endpoints in the registered EndpointDataSource may not have been built yet. this is important
        // for the api explorer extensions (ex: openapi). the following is the same setup that would occur
        // through via di, but the IEndpointRouteBuilder is expected to be the WebApplication used during
        // setup. unfortunately, the behavior cannot simply be changed by replacing IApiVersionDescriptionProvider
        // in the container for minimal apis, but that is not a common scenario. all the types and pieces
        // necessary to change this behavior is still possible outside of this method, but it's on the developer
        var source = new CompositeEndpointDataSource( endpoints.DataSources );
        var policyManager = endpoints.ServiceProvider.GetRequiredService<ISunsetPolicyManager>();
        var options = endpoints.ServiceProvider.GetRequiredService<IOptions<ApiExplorerOptions>>();
        var provider = new DefaultApiVersionDescriptionProvider( source, policyManager, options );

        return provider.ApiVersionDescriptions;
    }
}