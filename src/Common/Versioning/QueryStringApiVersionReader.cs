#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using Routing;
    using System;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents a service API version reader that reads the value from the query string in a URL.
    /// </summary>
    public partial class QueryStringApiVersionReader : IApiVersionReader
    {
        string parameterName = "api-version";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringApiVersionReader"/> class.
        /// </summary>
        public QueryStringApiVersionReader() { }

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
        /// Gets or sets the name of the query parameter to read the service API version from.
        /// </summary>
        /// <value>The name of the query parameter to read the service API version from.
        /// The default value is "api-version".</value>
        public string ParameterName
        {
            get
            {
                Contract.Ensures( !string.IsNullOrEmpty( parameterName ) );
                return parameterName;
            }
            set
            {
                Arg.NotNullOrEmpty( value, nameof( value ) );
                parameterName = value;
            }
        }
    }
}