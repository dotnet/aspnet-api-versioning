namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using static System.Linq.Enumerable;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    public partial class ActionApiVersionConventionBuilderBase : IApiVersionConvention<HttpActionDescriptor>
    {
        /// <summary>
        /// Applies the builder conventions to the specified controller action.
        /// </summary>
        /// <param name="actionDescriptor">The <see cref="HttpActionDescriptor">action descriptor</see>
        /// to apply the conventions to.</param>
        public virtual void ApplyTo( HttpActionDescriptor actionDescriptor )
        {
            if ( actionDescriptor == null )
            {
                throw new ArgumentNullException( nameof( actionDescriptor ) );
            }

            var attributes = new List<object>();

            attributes.AddRange( actionDescriptor.GetCustomAttributes<IApiVersionNeutral>( inherit: true ) );
            attributes.AddRange( actionDescriptor.GetCustomAttributes<IApiVersionProvider>( inherit: false ) );
            MergeAttributesWithConventions( attributes );

            if ( VersionNeutral )
            {
                actionDescriptor.SetProperty( ApiVersionModel.Neutral );
                return;
            }

            var versionModel = default( ApiVersionModel );

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

            actionDescriptor.SetProperty( versionModel );
        }
    }
}