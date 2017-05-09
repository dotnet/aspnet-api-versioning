namespace Microsoft.AspNetCore.Mvc
{
    using Http;
    using Microsoft.AspNetCore.Mvc.Routing;
    using System;
    using System.Diagnostics.Contracts;
    using Versioning;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpContext"/> class.
    /// </summary>
    [CLSCompliant( false )]
    public static class HttpContextExtensions
    {
        const string ApiVersionPropertiesKey = "MS_" + nameof( ApiVersionRequestProperties );

        /// <summary>
        /// Gets the current API versioning request properties.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext">HTTP context</see> to get the API versioning properties for.</param>
        /// <returns>The current <see cref="ApiVersionRequestProperties">API versioning properties</see>.</returns>
        public static ApiVersionRequestProperties ApiVersionProperties( this HttpContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( Contract.Result<ApiVersionRequestProperties>() != null );

            if ( !context.Items.TryGetValue( ApiVersionPropertiesKey, out ApiVersionRequestProperties properties ) )
            {
                context.Items[ApiVersionPropertiesKey] = properties = new ApiVersionRequestProperties( context );
            }

            return properties;
        }

        /// <summary>
        /// Gets the current API version requested.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext">HTTP context</see> to get the API version for.</param>
        /// <returns>The requested <see cref="ApiVersion">API version</see> or <c>null</c>.</returns>
        /// <remarks>This method will return <c>null</c> no service API version was requested or the requested
        /// service API version is in an invalid format.</remarks>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public static ApiVersion GetRequestedApiVersion( this HttpContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            return context.ApiVersionProperties().ApiVersion;
        }
    }
}