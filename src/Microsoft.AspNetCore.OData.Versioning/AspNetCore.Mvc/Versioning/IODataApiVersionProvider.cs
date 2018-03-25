namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the behavior of an object that provides OData API versions.
    /// </summary>
    public interface IODataApiVersionProvider
    {
        /// <summary>
        /// Gets the collection of all supported API versions.
        /// </summary>
        /// <value>A <see cref="IReadOnlyCollection{T}">read-only collection</see> of supported
        /// <see cref="ApiVersion">API versions</see>.</value>
        IReadOnlyCollection<ApiVersion> SupportedApiVersions { get; }

        /// <summary>
        /// Gets the collection of all deprecated API versions.
        /// </summary>
        /// <value>A <see cref="IReadOnlyCollection{T}">read-only collection</see> of deprecated
        /// <see cref="ApiVersion">API versions</see>.</value>
        IReadOnlyCollection<ApiVersion> DeprecatedApiVersions { get; }
    }
}