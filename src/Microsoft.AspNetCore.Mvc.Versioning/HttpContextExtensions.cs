namespace Microsoft.AspNetCore.Mvc
{
    using Http;
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using Versioning;
    using static ApiVersion;
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpContext"/> class.
    /// </summary>
    [CLSCompliant( false )]
    public static class HttpContextExtensions
    {
        private const string ApiVersionKey = "MS_" + nameof( ApiVersion );

        /// <summary>
        /// Gets the current raw, unparsed service API version requested.
        /// </summary>
        /// <param name="context">The current <see cref="HttpContext">HTTP context</see> to get the API version for.</param>
        /// <returns>The raw, unparsed service API version or <c>null</c> if no service API version was requested.</returns>
        /// <remarks>This method is primarily meant for internal use and is generally only useful for instrumentation purposes.
        /// It is recommended that you use the <see cref="GetRequestedApiVersion(HttpContext)"/> instead.</remarks>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        [EditorBrowsable( Never )]
        public static string GetRawRequestedApiVersion( this HttpContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            var reader = (IApiVersionReader) context.RequestServices.GetService( typeof( IApiVersionReader ) ) ?? new QueryStringApiVersionReader();
            return reader.Read( context.Request );
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

            var version = default( ApiVersion );

            if ( context.Items.TryGetValue( ApiVersionKey, out version ) )
            {
                return version;
            }

            var value = context.GetRawRequestedApiVersion();

            if ( !TryParse( value, out version ) )
            {
                version = null;
            }

            context.Items[ApiVersionKey] = version;
            return version;
        }

        internal static void SetRequestedApiVersion( this HttpContext context, ApiVersion version )
        {
            Contract.Requires( context != null );

            if ( version == null )
            {
                context.Items.Remove( ApiVersionKey );
            }
            else
            {
                context.Items[ApiVersionKey] = version;
            }
        }
    }
}
