namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the base implementation of a service API version reader.
    /// </summary>
    [CLSCompliant( false )]
    public abstract partial class ApiVersionReader : IApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        public abstract string Read( HttpRequest request );

        /// <summary>
        /// Reads the service API version value from the query string of a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to read the API version from.</param>
        /// <param name="parameterName">The name of the query parameter to read from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        protected static string ReadFromQueryString( HttpRequest request, string parameterName )
        {
            Arg.NotNull( request, nameof( request ) );
            Arg.NotNullOrEmpty( parameterName, nameof( parameterName ) );
            return request.Query[parameterName];
        }

        /// <summary>
        /// Reads the service API version value from a HTTP header of a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to read the API version from.</param>
        /// <param name="headerNames">The <see cref="IEnumerable{T}">sequence</see> of HTTP header names to extract the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        /// <remarks>When multiple HTTP header names are specified, the first matching HTTP header will be used.</remarks>
        protected static string ReadFromHeader( HttpRequest request, IEnumerable<string> headerNames )
        {
            Arg.NotNull( request, nameof( request ) );

            var headers = request.Headers;

            foreach ( var name in headerNames )
            {
                var version = headers[name].FirstOrDefault();

                if ( !string.IsNullOrEmpty( version ) )
                {
                    return version;
                }
            }

            return null;
        }
    }
}
