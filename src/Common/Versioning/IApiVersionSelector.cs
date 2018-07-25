#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
#if !WEBAPI
    using Microsoft.AspNetCore.Http;
#endif
    using System;
#if WEBAPI
    using HttpRequest = System.Net.Http.HttpRequestMessage;
#endif

    /// <summary>
    /// Defines the behavior of an API version selector.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public interface IApiVersionSelector
    {
        /// <summary>
        /// Selects an API version given the specified HTTP request and API version information.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest">HTTP request</see> to select the version for.</param>
        /// <param name="model">The <see cref="ApiVersionModel">model</see> to select the version from.</param>
        /// <returns>The selected <see cref="ApiVersion">API version</see>.</returns>
        ApiVersion SelectVersion( HttpRequest request, ApiVersionModel model );
    }
}