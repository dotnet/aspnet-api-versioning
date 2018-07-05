﻿namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Http;
    using System;
    using System.Collections.Generic;
    using static System.String;

    /// <content>
    /// Provides the implementation for ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class QueryStringApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public virtual string Read( HttpRequest request )
        {
            Arg.NotNull( request, nameof( request ) );

            var versions = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

            foreach ( var parameterName in ParameterNames )
            {
                var values = request.Query[parameterName];

                foreach ( var value in values )
                {
                    if ( !IsNullOrEmpty( value ) )
                    {
                        versions.Add( value );
                    }
                }
            }

            return versions.EnsureZeroOrOneApiVersions();
        }
    }
}