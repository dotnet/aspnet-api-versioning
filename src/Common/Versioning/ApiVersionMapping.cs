#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;

    /// <summary>
    /// Represents the possible types of API version mappings.
    /// </summary>
    public enum ApiVersionMapping
    {
        /// <summary>
        /// Indicates no mapping.
        /// </summary>
        None,

        /// <summary>
        /// Indicates an explicit mapping.
        /// </summary>
        Explicit,

        /// <summary>
        /// Indicates an implicit mapping.
        /// </summary>
        Implicit,
    }
}