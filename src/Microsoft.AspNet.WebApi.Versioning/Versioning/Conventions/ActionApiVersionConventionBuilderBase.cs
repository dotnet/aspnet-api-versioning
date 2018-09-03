namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;

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
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );

            MappedVersions.AddRange( from provider in actionDescriptor.GetCustomAttributes<IApiVersionProvider>()
                                     where !provider.AdvertiseOnly && !provider.Deprecated
                                     from version in provider.Versions
                                     select version );

            var (supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions) =
                actionDescriptor.GetProperty<Tuple<IEnumerable<ApiVersion>,
                                                   IEnumerable<ApiVersion>,
                                                   IEnumerable<ApiVersion>,
                                                   IEnumerable<ApiVersion>>>();

            var versionModel = new ApiVersionModel(
                declaredVersions: MappedVersions,
                supportedVersions,
                deprecatedVersions,
                advertisedVersions,
                deprecatedAdvertisedVersions );

            actionDescriptor.SetProperty( versionModel );
        }
    }
}