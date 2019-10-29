#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
#if WEBAPI
    using Microsoft.Web.Http.Routing;
#else
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Net.Http.Headers;
#endif
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if WEBAPI
    using System.Net.Http.Headers;
#else
    using MediaTypeWithQualityHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;
#endif
    using static ApiVersionParameterLocation;

    /// <summary>
    /// Represents a service API version reader that reads the value from a media type HTTP header in the request.
    /// </summary>
    public partial class MediaTypeApiVersionReader : IApiVersionReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeApiVersionReader"/> class.
        /// </summary>
        public MediaTypeApiVersionReader() => ParameterName = "v";

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaTypeApiVersionReader"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the query string parameter to read the service API version from.</param>
        public MediaTypeApiVersionReader( string parameterName ) => ParameterName = parameterName;

        /// <summary>
        /// Gets or sets the name of the media type parameter to read the service API version from.
        /// </summary>
        /// <value>The name of the media type parameter to read the service API version from.
        /// The default value is "v".</value>
        public string ParameterName { get; set; }

        /// <summary>
        /// Reads the requested API version from the HTTP Accept header.
        /// </summary>
        /// <param name="accept">The <see cref="IEnumerable{T}">sequence</see> of Accept
        /// <see cref="MediaTypeWithQualityHeaderValue">headers</see> to read from.</param>
        /// <returns>The API version read or <c>null</c>.</returns>
        /// <remarks>The default implementation will return the first defined API version ranked by the media type
        /// quality parameter.</remarks>
        protected virtual string? ReadAcceptHeader( IEnumerable<MediaTypeWithQualityHeaderValue> accept )
        {
            var comparer = StringComparer.OrdinalIgnoreCase;
            var contentTypes = from entry in accept
                               orderby entry.Quality descending
                               group entry by entry.MediaType;

            foreach ( var contentType in contentTypes )
            {
                foreach ( var entry in contentType )
                {
                    foreach ( var parameter in entry.Parameters )
                    {
#if WEBAPI
                        if ( comparer.Equals( parameter.Name, ParameterName ) )
                        {
                            return parameter.Value;
                        }
#else
                        if ( comparer.Equals( parameter.Name.Value, ParameterName ) )
                        {
                            return parameter.Value.Value;
                        }
#endif
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Reads the requested API version from the HTTP Content-Type header.
        /// </summary>
        /// <param name="contentType">The Content-Type <see cref="MediaTypeHeaderValue">header</see> to read from.</param>
        /// <returns>The API version read or <c>null</c>.</returns>
        protected virtual string? ReadContentTypeHeader( MediaTypeHeaderValue contentType )
        {
            if ( contentType == null )
            {
                throw new ArgumentNullException( nameof( contentType ) );
            }

            var comparer = StringComparer.OrdinalIgnoreCase;

            foreach ( var parameter in contentType.Parameters )
            {
#if WEBAPI
                if ( comparer.Equals( parameter.Name, ParameterName ) )
                {
                    return parameter.Value;
                }
#else
                if ( comparer.Equals( parameter.Name.Value, ParameterName ) )
                {
                    return parameter.Value.Value;
                }
#endif
            }

            return null;
        }

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

            context.AddParameter( ParameterName, MediaTypeParameter );
        }
    }
}