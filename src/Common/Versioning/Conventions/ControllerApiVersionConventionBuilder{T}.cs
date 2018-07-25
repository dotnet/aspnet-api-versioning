﻿#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Represents a builder for API versions applied to a controller.
    /// </summary>
#pragma warning disable SA1619 // Generic type parameters should be documented partial class; false positive
    public partial class ControllerApiVersionConventionBuilder<T> : ControllerApiVersionConventionBuilderBase, IControllerConventionBuilder
#pragma warning restore SA1619
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilder{T}"/> class.
        /// </summary>
        public ControllerApiVersionConventionBuilder() => ActionBuilders = new ActionApiVersionConventionBuilderCollection<T>( this );

        /// <summary>
        /// Gets a collection of controller action convention builders.
        /// </summary>
        /// <value>A <see cref="ActionApiVersionConventionBuilderCollection{T}">collection</see> of
        /// <see cref="ActionApiVersionConventionBuilder{T}">controller action convention builders</see>.</value>
        protected virtual ActionApiVersionConventionBuilderCollection<T> ActionBuilders { get; }

        /// <summary>
        /// Indicates that the controller is API version-neutral.
        /// </summary>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder<T> IsApiVersionNeutral()
        {
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder<T>>() != null );
            VersionNeutral = true;
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder<T> HasApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder<T>>() != null );

            SupportedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder<T> HasDeprecatedApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder<T>>() != null );

            DeprecatedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder<T> AdvertisesApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder<T>>() != null );

            AdvertisedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured controller.
        /// </summary>
        /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the controller.</param>
        /// <returns>The original <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder<T> AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder<T>>() != null );

            DeprecatedAdvertisedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        [EditorBrowsable( Never )]
        public virtual ActionApiVersionConventionBuilder<T> Action( MethodInfo actionMethod )
        {
            Arg.NotNull( actionMethod, nameof( actionMethod ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );
            return ActionBuilders.GetOrAdd( actionMethod );
        }

        void IControllerConventionBuilder.IsApiVersionNeutral() => IsApiVersionNeutral();

        void IControllerConventionBuilder.HasApiVersion( ApiVersion apiVersion ) => HasApiVersion( apiVersion );

        void IControllerConventionBuilder.HasDeprecatedApiVersion( ApiVersion apiVersion ) => HasDeprecatedApiVersion( apiVersion );

        void IControllerConventionBuilder.AdvertisesApiVersion( ApiVersion apiVersion ) => AdvertisesApiVersion( apiVersion );

        void IControllerConventionBuilder.AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) => AdvertisesDeprecatedApiVersion( apiVersion );
    }
}