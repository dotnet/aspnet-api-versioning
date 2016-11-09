using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using ApplicationModels;
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using ControllerVersionInfo = Tuple<IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>>;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Core.
    /// </content>
    /// <typeparam name="T">The <see cref="Type">type</see> of <see cref="ICommonModel">model</see>.</typeparam>
    [CLSCompliant( false )]
    public partial class ControllerApiVersionConventionBuilder<T> : IApiVersionConvention<ControllerModel>, IActionConventionBuilder<T>
    {
        /// <summary>
        /// Applies the builder conventions to the specified controller.
        /// </summary>
        /// <param name="controllerModel">The <see cref="ControllerModel">controller model</see> to apply the conventions to.</param>
        public void ApplyTo( ControllerModel controllerModel )
        {
            Arg.NotNull( controllerModel, nameof( controllerModel ) );

            var controllerVersionInfo = ApplyControllerConventions( controllerModel );

            if ( ActionBuilders.Count > 0 )
            {
                ApplyActionConventions( controllerModel, controllerVersionInfo );
            }
        }

        private ControllerVersionInfo ApplyControllerConventions( ControllerModel controllerModel )
        {
            Contract.Requires( controllerModel != null );
            Contract.Ensures( Contract.Result<ControllerVersionInfo>() != null );

            MergeAttributesWithConventions( controllerModel );

            var model = new ApiVersionModel( VersionNeutral, supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions );

            controllerModel.SetProperty( model );

            return new ControllerVersionInfo( supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions );
        }

        private void MergeAttributesWithConventions( ControllerModel controllerModel )
        {
            Contract.Requires( controllerModel != null );

            if ( VersionNeutral )
            {
                return;
            }

            var providers = controllerModel.Attributes.OfType<IApiVersionProvider>().ToArray();

            if ( providers.Length == 0 )
            {
                return;
            }

            supportedVersions.UnionWith( from provider in providers
                                         where !provider.AdvertiseOnly && !provider.Deprecated
                                         from version in provider.Versions
                                         select version );

            deprecatedVersions.UnionWith( from provider in providers
                                          where !provider.AdvertiseOnly && provider.Deprecated
                                          from version in provider.Versions
                                          select version );

            advertisedVersions.UnionWith( from provider in providers
                                          where provider.AdvertiseOnly && !provider.Deprecated
                                          from version in provider.Versions
                                          select version );

            deprecatedAdvertisedVersions.UnionWith( from provider in providers
                                                    where provider.AdvertiseOnly && provider.Deprecated
                                                    from version in provider.Versions
                                                    select version );
        }

        private void ApplyActionConventions( ControllerModel controller, ControllerVersionInfo controllerVersionInfo )
        {
            Contract.Requires( controller != null );

            foreach ( var action in controller.Actions )
            {
                var key = action.ActionMethod;
                var actionBuilder = default( ActionApiVersionConventionBuilder<T> );

                action.SetProperty( controller );

                if ( ActionBuilders.TryGetValue( key, out actionBuilder ) )
                {
                    action.SetProperty( controllerVersionInfo );
                    actionBuilder.ApplyTo( action );
                    action.SetProperty( default( ControllerVersionInfo ) );
                }
                else
                {
                    action.SetProperty( new ApiVersionModel( controller, action ) );
                }
            }
        }
    }
}