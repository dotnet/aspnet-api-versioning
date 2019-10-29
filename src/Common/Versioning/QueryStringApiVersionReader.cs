#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
#if WEBAPI
    using Microsoft.Web.Http.Versioning;
#else
    using Microsoft.AspNetCore.Mvc.Versioning;
#endif
    using System;
    using System.Collections.Generic;
    using static ApiVersionParameterLocation;
    using static System.StringComparer;

    /// <summary>
    /// Represents a service API version reader that reads the value from the query string in a URL.
    /// </summary>
    public partial class QueryStringApiVersionReader : IApiVersionReader
    {
        const string DefaultQueryParameterName = "api-version";

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringApiVersionReader"/> class.
        /// </summary>
        /// <remarks>This constructor always adds the "api-version" query string parameter.</remarks>
        public QueryStringApiVersionReader() => ParameterNames.Add( DefaultQueryParameterName );

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringApiVersionReader"/> class.
        /// </summary>
        /// <param name="parameterNames">A <see cref="IEnumerable{T}">sequence</see> of query string parameter names to read the service API version from.</param>
        /// <remarks>This constructor adds the "api-version" query string parameter if no other query parameter names are specified.</remarks>
        public QueryStringApiVersionReader( IEnumerable<string> parameterNames )
        {
            ParameterNames.AddRange( parameterNames ?? throw new ArgumentNullException( nameof( parameterNames ) ) );

            if ( ParameterNames.Count == 0 )
            {
                ParameterNames.Add( DefaultQueryParameterName );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringApiVersionReader"/> class.
        /// </summary>
        /// <param name="parameterNames">An array of query string parameter names to read the service API version from.</param>
        /// <remarks>This constructor adds the "api-version" query string parameter if no other query parameter names are specified.</remarks>
        public QueryStringApiVersionReader( params string[] parameterNames )
        {
            ParameterNames.AddRange( parameterNames );

            if ( ParameterNames.Count == 0 )
            {
                ParameterNames.Add( DefaultQueryParameterName );
            }
        }

        /// <summary>
        /// Gets a collection of HTTP header names that the service API version can be read from.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of HTTP header names.</value>
        /// <remarks>HTTP header names are evaluated in a case-insensitive manner.</remarks>
        public ICollection<string> ParameterNames { get; } = new HashSet<string>( OrdinalIgnoreCase );

        /// <summary>
        /// Provides API version parameter descriptions supported by the current reader using the supplied provider.
        /// </summary>
        /// <param name="context">The <see cref="IApiVersionParameterDescriptionContext">context</see> used to add API version parameter descriptions.</param>
        public virtual void AddParameters( IApiVersionParameterDescriptionContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            foreach ( var name in ParameterNames )
            {
                context.AddParameter( name, Query );
            }
        }
    }
}