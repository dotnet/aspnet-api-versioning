#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static ApiVersionProviderOptions;

    /// <summary>
    /// Represents the base implementation of an API version convention builder.
    /// </summary>
    public abstract partial class ApiVersionConventionBuilderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionConventionBuilderBase"/> class.
        /// </summary>
        protected ApiVersionConventionBuilderBase() { }

        /// <summary>
        /// Gets or sets a value indicating whether the current controller is API version-neutral.
        /// </summary>
        /// <value>True if the current controller is API version-neutral; otherwise, false. The default value is <c>false</c>.</value>
        protected bool VersionNeutral { get; set; }

        /// <summary>
        /// Gets the collection of API versions supported by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of supported <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> SupportedVersions { get; } = new HashSet<ApiVersion>();

        /// <summary>
        /// Gets the collection of API versions deprecated by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of deprecated <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> DeprecatedVersions { get; } = new HashSet<ApiVersion>();

        /// <summary>
        /// Gets the collection of API versions advertised by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of advertised <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> AdvertisedVersions { get; } = new HashSet<ApiVersion>();

        /// <summary>
        /// Gets the collection of API versions advertised and deprecated by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of advertised and deprecated <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> DeprecatedAdvertisedVersions { get; } = new HashSet<ApiVersion>();

        /// <summary>
        /// Merges API version information from the specified attributes with the current conventions.
        /// </summary>
        /// <param name="attributes">The <see cref="IEnumerable{T}">sequence</see> of attributes to merge.</param>
        protected virtual void MergeAttributesWithConventions( IEnumerable<object> attributes )
        {
            if ( VersionNeutral |= attributes.OfType<IApiVersionNeutral>().Any() )
            {
                return;
            }

            const ApiVersionProviderOptions DeprecatedAdvertised = Deprecated | Advertised;
            var providers = attributes.OfType<IApiVersionProvider>();

            foreach ( var provider in providers )
            {
                switch ( provider.Options )
                {
                    case None:
                        SupportedVersions.UnionWith( provider.Versions );
                        break;
                    case Deprecated:
                        DeprecatedVersions.UnionWith( provider.Versions );
                        break;
                    case Advertised:
                        AdvertisedVersions.UnionWith( provider.Versions );
                        break;
                    case DeprecatedAdvertised:
                        DeprecatedAdvertisedVersions.UnionWith( provider.Versions );
                        break;
                }
            }
        }
    }
}