namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;
    using static System.String;
    using static System.StringComparer;

    /// <content>
    /// Provides the implementation for ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class HeaderApiVersionReader
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

            var count = HeaderNames.Count;

            if ( count == 0 )
            {
                return default;
            }

            var version = default( string );
            var versions = default( SortedSet<string> );
            var names = new string[count];
            var headers = request.Headers;

            HeaderNames.CopyTo( names, 0 );

            for ( var i = 0; i < count; i++ )
            {
                if ( !headers.TryGetValue( names[i], out var values ) )
                {
                    continue;
                }

                for ( var j = 0; j < values.Count; j++ )
                {
                    var value = values[j];

                    if ( IsNullOrEmpty( value ) )
                    {
                        continue;
                    }

                    if ( version == null )
                    {
                        version = value;
                    }
                    else if ( versions == null )
                    {
                        versions = new SortedSet<string>( OrdinalIgnoreCase ) { version, value };
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