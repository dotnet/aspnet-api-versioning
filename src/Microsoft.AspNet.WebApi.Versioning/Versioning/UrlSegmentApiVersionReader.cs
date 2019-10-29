namespace Microsoft.Web.Http.Versioning
{
    using System.Net.Http;
    using System.Web.Http;

    /// <content>
    /// Provides the implementation for ASP.NET Web API.
    /// </content>
    public partial class UrlSegmentApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public virtual string? Read( HttpRequestMessage request )
        {
            if ( reentrant )
            {
                return null;
            }

            reentrant = true;
            var value = request.ApiVersionProperties().RawRequestedApiVersion;
            reentrant = false;

            return value;
        }
    }
}