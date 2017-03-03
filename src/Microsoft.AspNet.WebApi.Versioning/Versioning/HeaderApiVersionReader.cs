namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using static System.String;

    /// <content>
    /// Provides the implementation for ASP.NET Web API.
    /// </content>
    public partial class HeaderApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public virtual string Read( HttpRequestMessage request )
        {
            Arg.NotNull( request, nameof( request ) );

            var headers = request.Headers;
            var versions = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

            foreach ( var name in HeaderNames )
            {
                var values = default( IEnumerable<string> );

                if ( headers.TryGetValues( name, out values ) )
                {
                    versions.AddRange( values.Where( v => !IsNullOrEmpty( v ) ) );
                }
            }

            return versions.EnsureZeroOrOneApiVersions();
        }
    }
}