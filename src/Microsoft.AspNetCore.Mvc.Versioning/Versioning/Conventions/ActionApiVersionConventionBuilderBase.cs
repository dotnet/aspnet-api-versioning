using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using ApplicationModels;
    using System;
    using System.Linq;
    using ControllerVersionInfo = Tuple<IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>>;

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
        public void ApplyTo( ActionModel actionModel )
        {
            Arg.NotNull( actionModel, nameof( actionModel ) );

            MappedVersions.AddRange( from provider in actionModel.Attributes.OfType<IApiVersionProvider>()
                                     where !provider.AdvertiseOnly && !provider.Deprecated
                                     from version in provider.Versions
                                     select version );

            var controllerVersionInfo = actionModel.GetProperty<ControllerVersionInfo>();
            var versionModel = new ApiVersionModel(
                declaredVersions: MappedVersions,
                supportedVersions: controllerVersionInfo.Item1,
                deprecatedVersions: controllerVersionInfo.Item2,
                advertisedVersions: controllerVersionInfo.Item3,
                deprecatedAdvertisedVersions: controllerVersionInfo.Item4 );

            actionModel.SetProperty( versionModel );
        }
    }
}