namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;
    using System.Linq;
    using static System.Linq.Enumerable;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class ActionApiVersionConventionBuilderBase : IApiVersionConvention<ActionModel>
    {
        /// <summary>
        /// Applies the builder conventions to the specified controller action.
        /// </summary>
        /// <param name="actionModel">The <see cref="ActionModel">action model</see> to apply the conventions to.</param>
        public virtual void ApplyTo( ActionModel actionModel )
        {
            if ( actionModel == null )
            {
                throw new ArgumentNullException( nameof( actionModel ) );
            }

            MergeAttributesWithConventions( actionModel.Attributes );

            if ( VersionNeutral )
            {
                actionModel.SetProperty( ApiVersionModel.Neutral );
                return;
            }

            ApiVersionModel versionModel;

            if ( MappedVersions.Count == 0 )
            {
                var declaredVersions = SupportedVersions.Union( DeprecatedVersions );

                versionModel = new ApiVersionModel(
                    declaredVersions,
                    SupportedVersions,
                    DeprecatedVersions,
                    AdvertisedVersions,
                    DeprecatedAdvertisedVersions );
            }
            else
            {
                var emptyVersions = Empty<ApiVersion>();

                versionModel = new ApiVersionModel(
                    declaredVersions: MappedVersions,
                    supportedVersions: emptyVersions,
                    deprecatedVersions: emptyVersions,
                    advertisedVersions: emptyVersions,
                    deprecatedAdvertisedVersions: emptyVersions );
            }

            actionModel.SetProperty( versionModel );
        }
    }
}