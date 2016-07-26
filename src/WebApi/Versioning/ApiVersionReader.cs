namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using static System.StringComparison;

    /// <content>
    /// Provides the implementation for ASP.NET Web API.
    /// </content>
    public abstract partial class ApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        public abstract string Read( HttpRequestMessage request );

        /// <summary>
        /// Reads the service API version value from the query string of a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">HTTP request</see> to read the API version from.</param>
        /// <param name="parameterName">The name of the query parameter to read from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        protected static string ReadFromQueryString( HttpRequestMessage request, string parameterName )
        {
            Arg.NotNull( request, nameof( request ) );
            Arg.NotNullOrEmpty( parameterName, nameof( parameterName ) );

            var version = ( from pair in request.GetQueryNameValuePairs()
                            where parameterName.Equals( pair.Key, OrdinalIgnoreCase ) && pair.Value.Length > 0
                            select pair.Value ).FirstOrDefault();

            return version;
        }

        /// <summary>
        /// Reads the service API version value from a HTTP header of a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">HTTP request</see> to read the API version from.</param>
        /// <param name="headerNames">The <see cref="IEnumerable{T}">sequence</see> of HTTP header names to extract the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        /// <remarks>When multiple HTTP header names are specified, the first matching HTTP header will be used.</remarks>
        protected static string ReadFromHeader( HttpRequestMessage request, IEnumerable<string> headerNames )
        {
            Arg.NotNull( request, nameof( request ) );

            var headers = request.Headers;

            foreach ( var name in headerNames )
            {
                var version = headers.FirstOrDefault( name );

                if ( !string.IsNullOrEmpty( version ) )
                {
                    return version;
                }
            }

            return null;
        }
    }
}
