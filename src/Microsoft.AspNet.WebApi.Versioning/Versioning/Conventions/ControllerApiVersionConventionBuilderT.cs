namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    /// <typeparam name="T">The <see cref="Type">type</see> of <see cref="IHttpController">controller</see>.</typeparam>
    public partial class ControllerApiVersionConventionBuilder<T> : IApiVersionConvention<HttpControllerDescriptor>, IActionConventionBuilder<T> where T : IHttpController
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

            if ( ActionBuilders.Count > 0 )
            {
                ApplyActionConventions( controllerDescriptor );
            }
        }

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

                if ( ActionBuilders.TryGetValue( key, out var actionBuilder ) )
                {
                    actionBuilder.ApplyTo( actionDescriptor );
                }
            }
        }
    }
}