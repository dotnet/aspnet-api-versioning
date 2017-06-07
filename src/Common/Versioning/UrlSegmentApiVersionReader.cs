#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
#if WEBAPI
    using Routing;
#else
    using Microsoft.AspNetCore.Routing;
    using Routing;
#endif
    using System.Diagnostics.Contracts;
    using static ApiVersionParameterLocation;

    /// <summary>
    /// Represents a service API version reader that reads the value from a path segment in the request URL.
    /// </summary>
    public partial class UrlSegmentApiVersionReader : IApiVersionReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlSegmentApiVersionReader"/> class.
        /// </summary>
        public UrlSegmentApiVersionReader() { }

        /// <summary>
        /// Provides API version parameter descriptions supported by the current reader using the supplied provider.
        /// </summary>
        /// <param name="context">The <see cref="IApiVersionParameterDescriptionContext">context</see> used to add API version parameter descriptions.</param>
        public virtual void AddParmeters( IApiVersionParameterDescriptionContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            const string FromRouteValueName = null;
            context.AddParameter( FromRouteValueName, Path );
        }
    }
}