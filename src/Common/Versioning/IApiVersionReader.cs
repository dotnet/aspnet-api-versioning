#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
#if WEBAPI
    using HttpRequest = System.Net.Http.HttpRequestMessage;
#else
    using AspNetCore.Http;
#endif

    /// <summary>
    /// Defines the behavior of a service API version reader.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public interface IApiVersionReader : IApiVersionParameterSource
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        string Read( HttpRequest request );
    }
}