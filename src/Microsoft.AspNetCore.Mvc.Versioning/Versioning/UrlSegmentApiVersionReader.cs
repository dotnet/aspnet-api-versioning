namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using System;

    /// <content>
    /// Provides the implementation for ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class UrlSegmentApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public virtual string? Read( HttpRequest request )
        {
            if ( request == null )
            {
                throw new ArgumentNullException( nameof( request ) );
            }

            if ( reentrant )
            {
                return null;
            }

            reentrant = true;
            var feature = request.HttpContext.Features.Get<IApiVersioningFeature>();
            var value = feature.RawRequestedApiVersion;
            reentrant = false;

            return value;
        }
    }
}