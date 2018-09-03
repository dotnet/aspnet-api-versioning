namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    public partial class ControllerApiVersionConventionBuilderBase : IApiVersionConvention<HttpControllerDescriptor>
    {
        /// <summary>
        /// Applies the builder conventions to the specified controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller descriptor</see>
        /// to apply the conventions to.</param>
        public virtual void ApplyTo( HttpControllerDescriptor controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );
            ApplyActionConventions( controllerDescriptor, ApplyControllerConventions( controllerDescriptor ) );
        }

        /// <summary>
        /// Gets a value indicating whether the builder has any related action conventions.
        /// </summary>
        /// <value>True if the builder has related action conventions; otherwise, false.</value>
        protected abstract bool HasActionConventions { get; }

        /// <summary>
        /// Attempts to get the convention for the specified action method.
        /// </summary>
        /// <param name="method">The <see cref="MethodInfo">method</see> representing the action to retrieve the convention for.</param>
        /// <param name="convention">The retrieved <see cref="IApiVersionConvention{T}">convention</see> or <c>null</c>.</param>
        /// <returns>True if the convention was successfully retrieved; otherwise, false.</returns>
        protected abstract bool TryGetConvention( MethodInfo method, out IApiVersionConvention<HttpActionDescriptor> convention );

        static void ApplyNeutralModelToActions( HttpControllerDescriptor controller )
        {
            Contract.Requires( controller != null );

            var actionSelector = controller.Configuration.Services.GetActionSelector();
            var actions = actionSelector.GetActionMapping( controller ).SelectMany( g => g );

            foreach ( var action in actions )
            {
                action.SetProperty( ApiVersionModel.Neutral );
            }
        }

        Tuple<IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>> ApplyControllerConventions( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );
            Contract.Ensures( Contract.Result<Tuple<IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>>>() != null );

            MergeControllerAttributesWithConventions( controllerDescriptor );

            if ( VersionNeutral )
            {
                controllerDescriptor.SetProperty( ApiVersionModel.Neutral );
            }
            else
            {
                controllerDescriptor.SetProperty( new ApiVersionModel( VersionNeutral, supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions ) );
            }

            return Tuple.Create( supportedVersions.AsEnumerable(), deprecatedVersions.AsEnumerable(), advertisedVersions.AsEnumerable(), deprecatedAdvertisedVersions.AsEnumerable() );
        }

        void MergeControllerAttributesWithConventions( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );

            if ( VersionNeutral |= controllerDescriptor.GetCustomAttributes<IApiVersionNeutral>().Any() )
            {
                return;
            }

            var providers = controllerDescriptor.GetCustomAttributes<IApiVersionProvider>().ToArray();

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

        void ApplyActionConventions( HttpControllerDescriptor controller, Tuple<IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>> controllerVersionInfo )
        {
            Contract.Requires( controller != null );

            if ( VersionNeutral )
            {
                ApplyNeutralModelToActions( controller );
            }
            else
            {
                MergeActionAttributesWithConventions( controller, controllerVersionInfo );
            }
        }

        void MergeActionAttributesWithConventions( HttpControllerDescriptor controller, Tuple<IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>, IEnumerable<ApiVersion>> controllerVersionInfo )
        {
            Contract.Requires( controller != null );

            var actionSelector = controller.Configuration.Services.GetActionSelector();
            var actions = actionSelector.GetActionMapping( controller ).SelectMany( g => g.OfType<ReflectedHttpActionDescriptor>() );

            foreach ( var action in actions )
            {
                var key = action.MethodInfo;

                if ( TryGetConvention( key, out var actionConvention ) )
                {
                    action.SetProperty( controllerVersionInfo );
                    actionConvention.ApplyTo( action );
                    action.RemoveProperty( controllerVersionInfo );
                }
                else
                {
                    action.SetProperty( new ApiVersionModel( action ) );
                }
            }
        }
    }
}