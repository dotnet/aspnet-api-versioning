#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
#if WEBAPI
    using System.Net.Http.Headers;
#else
    using HttpResponseHeaders = Microsoft.AspNetCore.Http.IHeaderDictionary;
#endif

    /// <summary>
    /// Defines the behavior of an object that reports API versions as HTTP headers.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public interface IReportApiVersions
    {
        /// <summary>
        /// Reports the API versions defined in the specified models using the provided collection of HTTP headers.
        /// </summary>
        /// <param name="headers">The collection of <see cref="HttpResponseHeaders">HTTP response headers</see> used to report API versions.</param>
        /// <param name="apiVersionModel">The <see cref="ApiVersionModel">model</see> containing the API versions to report.</param>
        void Report( HttpResponseHeaders headers, ApiVersionModel apiVersionModel );

        /// <summary>
        /// Reports the API versions defined in the specified models using the provided collection of HTTP headers.
        /// </summary>
        /// <param name="headers">The collection of <see cref="HttpResponseHeaders">HTTP response headers</see> used to report API versions.</param>
        /// <param name="apiVersionModel">The load on-demand <see cref="ApiVersionModel">model</see> containing the API versions to report.</param>
        void Report( HttpResponseHeaders headers, Lazy<ApiVersionModel> apiVersionModel );
    }
}