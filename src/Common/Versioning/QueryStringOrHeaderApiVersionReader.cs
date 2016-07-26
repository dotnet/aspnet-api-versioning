#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using static System.StringComparer;

    /// <summary>
    /// Represents a service API version reader that reads the value from the query string in a URL or HTTP header.
    /// </summary>
    public partial class QueryStringOrHeaderApiVersionReader : QueryStringApiVersionReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringOrHeaderApiVersionReader"/> class.
        /// </summary>
        public QueryStringOrHeaderApiVersionReader()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringOrHeaderApiVersionReader"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter to read the service API version from.</param>
        public QueryStringOrHeaderApiVersionReader( string parameterName )
            : base( parameterName )
        {
        }

        /// <summary>
        /// Gets a collection of HTTP header names that the service API version can be read from.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of HTTP header names.</value>
        /// <remarks>HTTP header names are evaluated in a case-insensitive manner.</remarks>
        public ICollection<string> HeaderNames { get; } = new HashSet<string>( OrdinalIgnoreCase );
    }
}
