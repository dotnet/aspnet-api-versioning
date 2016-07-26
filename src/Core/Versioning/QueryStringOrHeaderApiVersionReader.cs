namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Http;
    using System;

    /// <content>
    /// Provides the implementation for ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class QueryStringOrHeaderApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        public override string Read( HttpRequest request )
        {
            var version = base.Read( request );

            if ( string.IsNullOrEmpty( version ) )
            {
                version = ReadFromHeader( request, HeaderNames );
            }

            return version;
        }
    }
}
