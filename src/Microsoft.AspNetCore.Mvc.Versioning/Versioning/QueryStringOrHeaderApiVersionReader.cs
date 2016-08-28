namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static System.String;

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
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public override string Read( HttpRequest request )
        {
            var version = base.Read( request );
            var versions = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

            if ( !IsNullOrEmpty( version ) )
            {
                versions.Add( version );
            }

            version = ReadFromHeader( request, HeaderNames );

            if ( !IsNullOrEmpty( version ) )
            {
                versions.Add( version );
            }

            return versions.EnsureZeroOrOneApiVersions();
        }
    }
}
