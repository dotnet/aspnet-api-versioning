namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
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
        public void ApplyTo( HttpControllerDescriptor controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );

            ApplyControllerConventions( controllerDescriptor );

            if ( HasActionConventions )
            {
                ApplyActionConventions( controllerDescriptor );
            }
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

        void ApplyControllerConventions( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );

            MergeAttributesWithConventions( controllerDescriptor );
            var model = new ApiVersionModel( VersionNeutral, supportedVersions, deprecatedVersions, advertisedVersions, deprecatedAdvertisedVersions );
            controllerDescriptor.SetConventionsApiVersionModel( model );
        }

        void MergeAttributesWithConventions( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );

            if ( VersionNeutral )
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

        void ApplyActionConventions( HttpControllerDescriptor controllerDescriptor )
        {
            Contract.Requires( controllerDescriptor != null );

            var actionSelector = controllerDescriptor.Configuration.Services.GetActionSelector();
            var actionDescriptors = actionSelector.GetActionMapping( controllerDescriptor ).SelectMany( g => g.OfType<ReflectedHttpActionDescriptor>() );

            foreach ( var actionDescriptor in actionDescriptors )
            {
                var key = actionDescriptor.MethodInfo;

                if ( TryGetConvention( key, out var actionConvention ) )
                {
                    actionConvention.ApplyTo( actionDescriptor );
                }
            }
        }
    }
}