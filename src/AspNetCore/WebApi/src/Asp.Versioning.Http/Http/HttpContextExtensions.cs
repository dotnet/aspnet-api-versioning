// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace Microsoft.AspNetCore.Http;

using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for the <see cref="HttpContext"/> class.
/// </summary>
[CLSCompliant( false )]
public static class HttpContextExtensions
{
    extension( HttpContext context )
    {
        /// <summary>
        /// Gets the API versioning feature associated with the current HTTP context.
        /// </summary>
        /// <returns>The current <see cref="IApiVersioningFeature">API versioning feature</see>.</returns>
        public IApiVersioningFeature ApiVersioningFeature
        {
            get
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
        }

        /// <summary>
        /// Gets the current API version requested.
        /// </summary>
        /// <returns>The requested <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
        /// <remarks>This method will return <c>null</c> no API version was requested or the requested
        /// API version is in an invalid format.</remarks>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public ApiVersion? RequestedApiVersion => context.ApiVersioningFeature.RequestedApiVersion;

        internal bool TryGetProblemDetailsService( [NotNullWhen( true )] out IProblemDetailsService? problemDetailsService )
        {
            problemDetailsService = context.RequestServices.GetService<IProblemDetailsService>();
            return problemDetailsService is not null;
        }
    }
}