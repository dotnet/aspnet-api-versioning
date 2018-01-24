#pragma warning disable SA1200 // Using directives should be placed correctly; false positive - required for inner, short-hand type aliasing
using System;
using System.Collections.Generic;
#pragma warning restore SA1200

namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using ApplicationModels;
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using ControllerVersionInfo = Tuple<IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>>;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class ControllerApiVersionConventionBuilderBase : IApiVersionConvention<ControllerModel>
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

        /// <summary>
        /// Attempts to get the convention for the specified action method.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo">method</see> representing the action to retrieve the convention for.</param>
        /// <param name="convention">The retrieved <see cref="IApiVersionConvention{T}">convention</see> or <c>null</c>.</param>
        /// <returns>True if the convention was successfully retrieved; otherwise, false.</returns>
        protected abstract bool TryGetConvention( MethodInfo method, out IApiVersionConvention<ActionModel> convention );

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

        static void ApplyNeutralModelToActions( ControllerModel controller )
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

                if ( TryGetConvention( key, out var actionConvention ) )
                {
                    action.SetProperty( controllerVersionInfo );
                    actionConvention.ApplyTo( action );
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