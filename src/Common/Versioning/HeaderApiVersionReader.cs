#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;
    using static ApiVersionParameterLocation;
    using static System.StringComparer;

    /// <summary>
    /// Represents a service API version reader that reads the value from a HTTP header.
    /// </summary>
    public partial class HeaderApiVersionReader : IApiVersionReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderApiVersionReader"/> class.
        /// </summary>
        public HeaderApiVersionReader() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderApiVersionReader"/> class.
        /// </summary>
        /// <param name="headerNames">A <see cref="IEnumerable{T}">sequence</see> of HTTP header names to read the service API version from.</param>
        public HeaderApiVersionReader( IEnumerable<string> headerNames ) =>
            HeaderNames.AddRange( headerNames ?? throw new ArgumentNullException( nameof( headerNames ) ) );

        /// <summary>
        /// Initializes a new instance of the <see cref="HeaderApiVersionReader"/> class.
        /// </summary>
        /// <param name="headerNames">An array of HTTP header names to read the service API version from.</param>
        public HeaderApiVersionReader( params string[] headerNames ) => HeaderNames.AddRange( headerNames );

        /// <summary>
        /// Gets a collection of HTTP header names that the service API version can be read from.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of HTTP header names.</value>
        /// <remarks>HTTP header names are evaluated in a case-insensitive manner.</remarks>
        public ICollection<string> HeaderNames { get; } = new HashSet<string>( OrdinalIgnoreCase );

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

            foreach ( var name in HeaderNames )
            {
                context.AddParameter( name, Header );
            }
        }
    }
}