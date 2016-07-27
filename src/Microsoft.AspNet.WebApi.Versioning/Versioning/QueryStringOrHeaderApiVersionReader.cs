namespace Microsoft.Web.Http.Versioning
{
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;

    /// <content>
    /// Provides the implementation for ASP.NET Web API.
    /// </content>
    public partial class QueryStringOrHeaderApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        public override string Read( HttpRequestMessage request )
        {
            Arg.NotNull( request, nameof( request ) );

            var version = base.Read( request );

            if ( string.IsNullOrEmpty( version ) )
            {
                version = ReadFromHeader( request, HeaderNames );
            }

            return version;
        }
    }
}
