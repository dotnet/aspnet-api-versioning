#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;

    /// <summary>
    /// Represents the possible API version provider options.
    /// </summary>
    [Flags]
    public enum ApiVersionProviderOptions
    {
        /// <summary>
        /// Indicates no options.
        /// </summary>
        None,

        /// <summary>
        /// Indicates the provided API versions are deprecated.
        /// </summary>
        Deprecated = 1,

        /// <summary>
        /// Indicates the provided API versions are only advertised.
        /// </summary>
        /// <remarks>Advertised service API versions indicate the existence of other versioned services,
        /// but the implementation of those services are implemented elsewhere.</remarks>
        Advertised = 2,

        /// <summary>
        /// Indicates the provided API versions are only mapped.
        /// </summary>
        /// <remarks>Mapped API versions indicate that the defined API versions are used for only meant
        /// to be used for mapping purposes. This option should not typically be combined with other options.</remarks>
        Mapped = 4,
    }
}