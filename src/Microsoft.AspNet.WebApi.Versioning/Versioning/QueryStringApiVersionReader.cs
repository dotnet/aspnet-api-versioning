namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

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
            if ( request == null )
            {
                throw new ArgumentNullException( nameof( request ) );
            }

            var count = ParameterNames.Count;

            if ( count == 0 )
            {
                return default;
            }

            var version = default( string );
            var versions = default( SortedSet<string> );
            var names = new string[count];
            var comparer = StringComparer.OrdinalIgnoreCase;

            ParameterNames.CopyTo( names, 0 );

            foreach ( var pair in request.GetQueryNameValuePairs() )
            {
                for ( var i = 0; i < count; i++ )
                {
                    var parameterName = names[i];
                    var value = pair.Value;

                    if ( value.Length == 0 || !comparer.Equals( parameterName, pair.Key ) )
                    {
                        continue;
                    }

                    if ( version == null )
                    {
                        version = value;
                    }
                    else if ( versions == null )
                    {
                        versions = new SortedSet<string>( comparer ) { version, value };
                    }
                    else
                    {
                        versions.Add( value );
                    }
                }
            }

            return versions == null ? version : versions.EnsureZeroOrOneApiVersions();
        }
    }
}