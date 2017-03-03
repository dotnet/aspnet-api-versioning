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

    /// <summary>
    /// Represents a service API version reader that reads the value from a path segment in the request URL.
    /// </summary>
    public partial class UrlSegmentApiVersionReader : IApiVersionReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlSegmentApiVersionReader"/> class.
        /// </summary>
        public UrlSegmentApiVersionReader() { }
    }
}