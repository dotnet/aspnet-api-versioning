#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.Contracts;
    using System.Reflection;
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Represents a builder for API versions applied to a controller.
    /// </summary>
    public partial class ControllerApiVersionConventionBuilder<T>
    {
        readonly HashSet<ApiVersion> supportedVersions = new HashSet<ApiVersion>();
        readonly HashSet<ApiVersion> deprecatedVersions = new HashSet<ApiVersion>();
        readonly HashSet<ApiVersion> advertisedVersions = new HashSet<ApiVersion>();
        readonly HashSet<ApiVersion> deprecatedAdvertisedVersions = new HashSet<ApiVersion>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerApiVersionConventionBuilder{T}"/> class.
        /// </summary>
        public ControllerApiVersionConventionBuilder() => ActionBuilders = new ActionApiVersionConventionBuilderCollection<T>( this );

        /// <summary>
        /// Gets or sets a value indicating whether the current controller is API version-neutral.
        /// </summary>
        /// <value>True if the current controller is API version-neutral; otherwise, false. The default value is <c>false</c>.</value>
        protected bool VersionNeutral { get; set; }

        /// <summary>
        /// Gets the collection of API versions supported by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of supported <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> SupportedVersions => supportedVersions;

        /// <summary>
        /// Gets the collection of API versions deprecated by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of deprecated <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> DeprecatedVersions => deprecatedVersions;

        /// <summary>
        /// Gets the collection of API versions advertised by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of advertised <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> AdvertisedVersions => advertisedVersions;

        /// <summary>
        /// Gets the collection of API versions advertised and deprecated by the current controller.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of advertised and deprecated <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> DeprecatedAdvertisedVersions => deprecatedAdvertisedVersions;

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

            supportedVersions.Add( apiVersion );
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

            deprecatedVersions.Add( apiVersion );
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

            advertisedVersions.Add( apiVersion );
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

            deprecatedAdvertisedVersions.Add( apiVersion );
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
    }
}