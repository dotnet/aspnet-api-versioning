#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Linq;

    /// <summary>
    /// Represents a service API version reader that reads the value from the query string in a URL.
    /// </summary>
    public partial class QueryStringApiVersionReader : ApiVersionReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringApiVersionReader"/> class.
        /// </summary>
        public QueryStringApiVersionReader()
        {
            ParameterName = "api-version";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringApiVersionReader"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter to read the service API version from.</param>
        public QueryStringApiVersionReader( string parameterName )
        {
            Arg.NotNullOrEmpty( parameterName, nameof( parameterName ) );
            ParameterName = parameterName;
        }

        /// <summary>
        /// Gets the name of the query parameter to read the service API version from.
        /// </summary>
        /// <value>The name of the query parameter to read the service API version from.
        /// The default value is "api-version".</value>
        protected string ParameterName { get; }
    }
}
