// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning.OpenApi;
using Microsoft.AspNetCore.Routing;

/// <summary>
/// Provides extension methods for <see cref="IEndpointRouteBuilder"/>.
/// </summary>
[CLSCompliant( false )]
public static partial class IEndpointRouteBuilderExtensions
{
    extension( IEndpointRouteBuilder endpoints )
    {
        /// <summary>
        /// Creates and returns a new route builder that includes versioned _Minimal APIs_ with OpenAPI support.
        /// </summary>
        /// <returns>A new <see cref="IEndpointRouteBuilder">endpoint route builder</see> with versioned, OpenAPI support.</returns>
        public IEndpointRouteBuilder IncludeVersionedEndpoints()
        {
            endpoints.ServiceProvider.GetRequiredService<ConfigureOpenApiOptions>().DataSources = endpoints.DataSources;
            return endpoints;
        }
    }
}