namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using static System.StringComparison;

    /// <content>
    /// Provides the implementation for ASP.NET Web API.
    /// </content>
    public partial class QueryStringApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public virtual string? Read( HttpRequestMessage request )
        {
            SortedSet<string>? versions = null;

            foreach ( var pair in request.GetQueryNameValuePairs() )
            {
                foreach ( var parameterName in ParameterNames )
                {
                    if ( parameterName.Equals( pair.Key, OrdinalIgnoreCase ) && pair.Value.Length > 0 )
                    {
                        ( versions ?? new SortedSet<string>( StringComparer.OrdinalIgnoreCase ) )
                            .Add( pair.Value );
                    }
                }
            }

            return versions.EnsureZeroOrOneApiVersions();
        }
    }
}