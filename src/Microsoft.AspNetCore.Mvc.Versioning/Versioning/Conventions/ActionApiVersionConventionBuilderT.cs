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
    /// <typeparam name="T">The <see cref="Type">type</see> of <see cref="ICommonModel">model</see>.</typeparam>
    [CLSCompliant( false )]
    public partial class ActionApiVersionConventionBuilder<T> : IApiVersionConvention<ActionModel>, IActionConventionBuilder<T>
    {
        /// <summary>
        /// Applies the builder conventions to the specified controller action.
        /// </summary>
        /// <param name="actionModel">The <see cref="ActionModel">action model</see> to apply the conventions to.</param>
        public void ApplyTo( ActionModel actionModel )
        {
            Arg.NotNull( actionModel, nameof( actionModel ) );

            mappedVersions.UnionWith( from provider in actionModel.Attributes.OfType<IApiVersionProvider>()
                                      where !provider.AdvertiseOnly && !provider.Deprecated
                                      from version in provider.Versions
                                      select version );

            var controllerControllerVersionInfo = actionModel.GetProperty<ControllerVersionInfo>();
            var versionModel = new ApiVersionModel(
                declared: mappedVersions,
                supported: controllerControllerVersionInfo.Item1,
                deprecated: controllerControllerVersionInfo.Item2,
                advertised: controllerControllerVersionInfo.Item3,
                deprecatedAdvertised: controllerControllerVersionInfo.Item4 );

            actionModel.SetProperty( versionModel );
        }
    }
}