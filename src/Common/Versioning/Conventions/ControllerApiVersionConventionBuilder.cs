﻿#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    /// Represents a builder for API versions applied to a controller.
    /// </summary>
    public partial class ControllerApiVersionConventionBuilder : ControllerApiVersionConventionBuilderBase, IControllerConventionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilder"/> class.
        /// </summary>
        /// <param name="controllerType">The <see cref="Type">type</see> of controller the convention builder is for.</param>
        public ControllerApiVersionConventionBuilder( Type controllerType )
        {
            Arg.NotNull( controllerType, nameof( controllerType ) );
#if WEBAPI
            var webApiController = typeof( System.Web.Http.Controllers.IHttpController );

            if ( !webApiController.IsAssignableFrom( controllerType ) )
            {
                throw new ArgumentException( SR.RequiredInterfaceNotImplemented.FormatDefault( controllerType, webApiController ), nameof( controllerType ) );
            }
#endif
            ControllerType = controllerType;
            ActionBuilders = new ActionApiVersionConventionBuilderCollection( this );
        }

        /// <summary>
        /// Gets the type of controller the convention builder is for.
        /// </summary>
        /// <value>The corresponding controller <see cref="Type">type</see>.</value>
        public Type ControllerType { get; }

        /// <summary>
        /// Gets a collection of controller action convention builders.
        /// </summary>
        /// <value>A <see cref="ActionApiVersionConventionBuilderCollection">collection</see> of
        /// <see cref="ActionApiVersionConventionBuilder">controller action convention builders</see>.</value>
        protected virtual ActionApiVersionConventionBuilderCollection ActionBuilders { get; }

        /// <summary>
        /// Indicates that the controller is API version-neutral.
        /// </summary>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder IsApiVersionNeutral()
        {
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );
            VersionNeutral = true;
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder HasApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            SupportedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder HasDeprecatedApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            DeprecatedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder AdvertisesApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            AdvertisedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            DeprecatedAdvertisedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder Action( MethodInfo actionMethod )
        {
            Arg.NotNull( actionMethod, nameof( actionMethod ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );
            return ActionBuilders.GetOrAdd( actionMethod );
        }

        void IControllerConventionBuilder.IsApiVersionNeutral() => IsApiVersionNeutral();

        void IControllerConventionBuilder.HasApiVersion( ApiVersion apiVersion ) => HasApiVersion( apiVersion );

        void IControllerConventionBuilder.HasDeprecatedApiVersion( ApiVersion apiVersion ) => HasDeprecatedApiVersion( apiVersion );

        void IControllerConventionBuilder.AdvertisesApiVersion( ApiVersion apiVersion ) => AdvertisesApiVersion( apiVersion );

        void IControllerConventionBuilder.AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) => AdvertisesDeprecatedApiVersion( apiVersion );
    }
}