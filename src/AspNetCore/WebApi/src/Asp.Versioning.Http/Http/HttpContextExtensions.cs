// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.AspNetCore.Http;

using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for the <see cref="HttpContext"/> class.
/// </summary>
[CLSCompliant( false )]
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the API versioning feature associated with the current HTTP context.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext">HTTP context</see> to get the API feature for.</param>
    /// <returns>The current <see cref="IApiVersioningFeature">API versioning feature</see>.</returns>
    public static IApiVersioningFeature ApiVersioningFeature( this HttpContext context )
    {
        ArgumentNullException.ThrowIfNull( context );

        var feature = context.Features.Get<IApiVersioningFeature>();

        if ( feature == null )
        {
            feature = new ApiVersioningFeature( context );
            context.Features.Set( feature );
        }

        return feature;
    }

    /// <summary>
    /// Gets the current API version requested.
    /// </summary>
    /// <param name="context">The current <see cref="HttpContext">HTTP context</see> to get the API version for.</param>
    /// <returns>The requested <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
    /// <remarks>This method will return <c>null</c> no API version was requested or the requested
    /// API version is in an invalid format.</remarks>
    /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
    public static ApiVersion? GetRequestedApiVersion( this HttpContext context ) => context.ApiVersioningFeature().RequestedApiVersion;

    internal static bool TryGetProblemDetailsService( this HttpContext context, [NotNullWhen( true )] out IProblemDetailsService? problemDetailsService )
    {
        problemDetailsService = context.RequestServices.GetService<IProblemDetailsService>();
        return problemDetailsService is not null;
    }
}