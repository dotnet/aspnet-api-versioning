#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the base implementation of a builder for API versions applied to a controller action.
    /// </summary>
    public partial class ActionApiVersionConventionBuilderBase
    {
        readonly HashSet<ApiVersion> mappedVersions = new HashSet<ApiVersion>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilderBase"/> class.
        /// </summary>
        protected ActionApiVersionConventionBuilderBase() { }

        /// <summary>
        /// Gets the collection of API versions mapped to the current action.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of mapped <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> MappedVersions => mappedVersions;
    }
}