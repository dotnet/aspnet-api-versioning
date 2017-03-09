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
            ApplyActionConventions( controllerModel, ApplyControllerConventions( controllerModel ) );
        }

        ControllerVersionInfo ApplyControllerConventions( ControllerModel controllerModel )
        {
            Contract.Requires( controllerModel != null );
            Contract.Ensures( Contract.Result<ControllerVersionInfo>() != null );

            MergeControllerAttributesWithConventions( controllerModel );

            if ( VersionNeutral )
            {
                controllerModel.SetProperty( ApiVersionModel.Neutral );
            }
            else
            {
                controllerModel.SetProperty( new ApiVersionModel( VersionNeutral, supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions ) );
            }

            return new ControllerVersionInfo( supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions );
        }

        void MergeControllerAttributesWithConventions( ControllerModel controllerModel )
        {
            Contract.Requires( controllerModel != null );

            if ( VersionNeutral |= controllerModel.Attributes.OfType<IApiVersionNeutral>().Any() )
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

        void ApplyActionConventions( ControllerModel controller, ControllerVersionInfo controllerVersionInfo )
        {
            Contract.Requires( controller != null );
            Contract.Requires( controllerVersionInfo != null );

            if ( VersionNeutral )
            {
                ApplyNeutralModelToActions( controller );
            }
            else
            {
                MergeActionAttributesWithConventions( controller, controllerVersionInfo );
            }
        }

        void ApplyNeutralModelToActions( ControllerModel controller )
        {
            Contract.Requires( controller != null );

            foreach ( var action in controller.Actions )
            {
                action.SetProperty( controller );
                action.SetProperty( ApiVersionModel.Neutral );
            }
        }

        void MergeActionAttributesWithConventions( ControllerModel controller, ControllerVersionInfo controllerVersionInfo )
        {
            Contract.Requires( controller != null );
            Contract.Requires( controllerVersionInfo != null );

            foreach ( var action in controller.Actions )
            {
                var key = action.ActionMethod;

                action.SetProperty( controller );

                if ( ActionBuilders.TryGetValue( key, out var actionBuilder ) )
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