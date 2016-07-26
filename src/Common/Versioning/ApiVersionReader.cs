#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Linq;

    /// <summary>
    /// Represents the base implementation of a service API version reader.
    /// </summary>
    public abstract partial class ApiVersionReader : IApiVersionReader
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionReader"/> class.
        /// </summary>
        protected ApiVersionReader()
        {
        }
    }
}
