namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
            Arg.NotNull( actionModel, nameof( actionModel ) );

            MappedVersions.AddRange( from provider in actionModel.Attributes.OfType<IApiVersionProvider>()
                                     where !provider.AdvertiseOnly && !provider.Deprecated
                                     from version in provider.Versions
                                     select version );

            var (supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions) =
                actionModel.GetProperty<Tuple<IEnumerable<ApiVersion>,
                                              IEnumerable<ApiVersion>,
                                              IEnumerable<ApiVersion>,
                                              IEnumerable<ApiVersion>>>();

            var versionModel = new ApiVersionModel(
                declaredVersions: MappedVersions,
                supportedVersions,
                deprecatedVersions,
                advertisedVersions,
                deprecatedAdvertisedVersions );

            actionModel.SetProperty( versionModel );
        }
    }
}