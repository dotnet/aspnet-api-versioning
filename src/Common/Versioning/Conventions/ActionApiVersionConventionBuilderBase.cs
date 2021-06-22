#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;
#if WEBAPI
    using static Microsoft.Web.Http.Versioning.ApiVersionProviderOptions;
#else
    using static Microsoft.AspNetCore.Mvc.Versioning.ApiVersionProviderOptions;
#endif

    /// <summary>
    /// Represents the base implementation of a builder for API versions applied to a controller action.
    /// </summary>
    public abstract partial class ActionApiVersionConventionBuilderBase : ApiVersionConventionBuilderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilderBase"/> class.
        /// </summary>
        protected ActionApiVersionConventionBuilderBase() { }

        /// <summary>
        /// Gets the collection of API versions mapped to the current action.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of mapped <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> MappedVersions { get; } = new HashSet<ApiVersion>();

        /// <inheritdoc />
        protected override void MergeAttributesWithConventions( IReadOnlyList<object> attributes )
        {
            if ( attributes == null )
            {
                throw new ArgumentNullException( nameof( attributes ) );
            }

            base.MergeAttributesWithConventions( attributes );

            for ( var i = 0; i < attributes.Count; i++ )
            {
                if ( attributes[i] is IApiVersionProvider provider && provider.Options == Mapped )
                {
                    MappedVersions.UnionWith( provider.Versions );
                }
            }
        }
    }
}