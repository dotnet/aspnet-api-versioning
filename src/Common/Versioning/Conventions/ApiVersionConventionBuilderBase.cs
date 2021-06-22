#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
#if WEBAPI
    using static Microsoft.Web.Http.Versioning.ApiVersionProviderOptions;
#else
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionProviderOptions;
#endif

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
        protected virtual void MergeAttributesWithConventions( IEnumerable<object> attributes ) =>
            MergeAttributesWithConventions( ( attributes as IReadOnlyList<object> ) ?? attributes.ToArray() );

        /// <summary>
        /// Merges API version information from the specified attributes with the current conventions.
        /// </summary>
        /// <param name="attributes">The <see cref="IReadOnlyList{T}">read-only list</see> of attributes to merge.</param>
        protected virtual void MergeAttributesWithConventions( IReadOnlyList<object> attributes )
        {
            if ( attributes == null )
            {
                throw new ArgumentNullException( nameof( attributes ) );
            }

            if ( VersionNeutral )
            {
                return;
            }

            const ApiVersionProviderOptions DeprecatedAdvertised = Deprecated | Advertised;
            var supported = default( List<ApiVersion> );
            var deprecated = default( List<ApiVersion> );
            var advertised = default( List<ApiVersion> );
            var deprecatedAdvertised = default( List<ApiVersion> );

            for ( var i = 0; i < attributes.Count; i++ )
            {
                switch ( attributes[i] )
                {
                    case IApiVersionNeutral:
                        VersionNeutral = true;
                        return;
                    case IApiVersionProvider provider:
                        List<ApiVersion> target;
                        IReadOnlyList<ApiVersion> source;

                        switch ( provider.Options )
                        {
                            case None:
                                target = supported ??= new();
                                source = provider.Versions;
                                break;
                            case Deprecated:
                                target = deprecated ??= new();
                                source = provider.Versions;
                                break;
                            case Advertised:
                                target = advertised ??= new();
                                source = provider.Versions;
                                break;
                            case DeprecatedAdvertised:
                                target = deprecatedAdvertised ??= new();
                                source = provider.Versions;
                                break;
                            default:
                                continue;
                        }

                        for ( var j = 0; j < source.Count; j++ )
                        {
                            target.Add( source[j] );
                        }

                        break;
                }
            }

            if ( supported is not null && supported.Count > 0 )
            {
                SupportedVersions.UnionWith( supported );
            }

            if ( deprecated is not null && deprecated.Count > 0 )
            {
                DeprecatedVersions.UnionWith( deprecated );
            }

            if ( advertised is not null && advertised.Count > 0 )
            {
                AdvertisedVersions.UnionWith( advertised );
            }

            if ( deprecatedAdvertised is not null && deprecatedAdvertised.Count > 0 )
            {
                DeprecatedAdvertisedVersions.UnionWith( deprecatedAdvertised );
            }
        }
    }
}