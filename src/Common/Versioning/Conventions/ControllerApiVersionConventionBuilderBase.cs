#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents the base implementation of a builder for API versions applied to a controller.
    /// </summary>
    public abstract partial class ControllerApiVersionConventionBuilderBase
    {
        readonly HashSet<ApiVersion> supportedVersions = new HashSet<ApiVersion>();
        readonly HashSet<ApiVersion> deprecatedVersions = new HashSet<ApiVersion>();
        readonly HashSet<ApiVersion> advertisedVersions = new HashSet<ApiVersion>();
        readonly HashSet<ApiVersion> deprecatedAdvertisedVersions = new HashSet<ApiVersion>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilderBase"/> class.
        /// </summary>
        protected ControllerApiVersionConventionBuilderBase() { }

        /// <summary>
        /// Gets or sets a value indicating whether the current controller is API version-neutral.
        /// </summary>
        /// <value>True if the current controller is API version-neutral; otherwise, false. The default value is <c>false</c>.</value>
        protected bool VersionNeutral { get; set; }

        /// <summary>
        /// Gets the collection of API versions supported by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of supported <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> SupportedVersions => supportedVersions;

        /// <summary>
        /// Gets the collection of API versions deprecated by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of deprecated <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> DeprecatedVersions => deprecatedVersions;

        /// <summary>
        /// Gets the collection of API versions advertised by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of advertised <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> AdvertisedVersions => advertisedVersions;

        /// <summary>
        /// Gets the collection of API versions advertised and deprecated by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of advertised and deprecated <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> DeprecatedAdvertisedVersions => deprecatedAdvertisedVersions;
    }
}