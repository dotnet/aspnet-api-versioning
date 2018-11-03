#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the behavior of a service <see cref="ApiVersion">API version</see> provider.
    /// </summary>
    public interface IApiVersionProvider
    {
        /// <summary>
        /// Gets the options associated with the provided API versions.
        /// </summary>
        ApiVersionProviderOptions Options { get; }

        /// <summary>
        /// Gets the defined API versions defined.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>.</value>
        IReadOnlyList<ApiVersion> Versions { get; }
    }
}