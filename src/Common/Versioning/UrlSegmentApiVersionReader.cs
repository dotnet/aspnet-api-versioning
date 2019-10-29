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
#endif
    using System;
    using static ApiVersionParameterLocation;

    /// <summary>
    /// Represents a service API version reader that reads the value from a path segment in the request URL.
    /// </summary>
    public partial class UrlSegmentApiVersionReader : IApiVersionReader
    {
        volatile bool reentrant;

        /// <summary>
        /// Initializes a new instance of the <see cref="UrlSegmentApiVersionReader"/> class.
        /// </summary>
        public UrlSegmentApiVersionReader() { }

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

            context.AddParameter( name: string.Empty, Path );
        }
    }
}