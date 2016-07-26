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
        /// Gets a value indicating whether the provided set of API versions are advertised only.
        /// </summary>
        /// <value>True if the provided set of API versions are only being advertised; otherwise, false.</value>
        /// <remarks>Advertised service API versions indicate the existence of other versioned services,
        /// but the implementation of those services are implemented elsewhere.</remarks>
        bool AdvertiseOnly { get; }

        /// <summary>
        /// Gets a value indicating whether the provided set of API versions are deprecated.
        /// </summary>
        /// <value>True if the specified set of API versions are deprecated; otherwise, false.</value>
        bool Deprecated { get; }

        /// <summary>
        /// Gets the defined API versions defined.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ApiVersion">API versions</see>.</value>
        IReadOnlyList<ApiVersion> Versions { get; }
    }
}

