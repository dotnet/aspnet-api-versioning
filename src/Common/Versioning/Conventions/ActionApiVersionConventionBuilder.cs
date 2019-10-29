#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Reflection;

    /// <summary>
    /// Represents a builder for API versions applied to a controller action.
    /// </summary>
#if !WEBAPI
    [CLSCompliant( false )]
#endif
    public class ActionApiVersionConventionBuilder : ActionApiVersionConventionBuilderBase, IActionConventionBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilder"/> class.
        /// </summary>
        /// <param name="controllerBuilder">The <see cref="ControllerApiVersionConventionBuilder">controller builder</see>
        /// the action builder belongs to.</param>
        public ActionApiVersionConventionBuilder( ControllerApiVersionConventionBuilder controllerBuilder ) =>
            ControllerBuilder = controllerBuilder;

        /// <summary>
        /// Gets the controller builder the action builder belongs to.
        /// </summary>
        /// <value>The associated <see cref="ControllerApiVersionConventionBuilder"/>.</value>
        protected ControllerApiVersionConventionBuilder ControllerBuilder { get; }

        /// <summary>
        /// Gets the type of controller the convention builder is for.
        /// </summary>
        /// <value>The corresponding controller <see cref="Type">type</see>.</value>
        public Type ControllerType => ControllerBuilder.ControllerType;

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder Action( MethodInfo actionMethod ) => ControllerBuilder.Action( actionMethod );

        /// <summary>
        /// Maps the specified API version to the configured controller action.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to map to the action.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder MapToApiVersion( ApiVersion apiVersion )
        {
            MappedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the action is API version-neutral.
        /// </summary>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder IsApiVersionNeutral()
        {
            VersionNeutral = true;
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is supported by the configured action.
        /// </summary>
        /// <param name="apiVersion">The supported <see cref="ApiVersion">API version</see> implemented by the action.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder HasApiVersion( ApiVersion apiVersion )
        {
            SupportedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is deprecated by the configured action.
        /// </summary>
        /// <param name="apiVersion">The deprecated <see cref="ApiVersion">API version</see> implemented by the action.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder HasDeprecatedApiVersion( ApiVersion apiVersion )
        {
            DeprecatedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised by the configured action.
        /// </summary>
        /// <param name="apiVersion">The advertised <see cref="ApiVersion">API version</see> not directly implemented by the action.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder AdvertisesApiVersion( ApiVersion apiVersion )
        {
            AdvertisedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Indicates that the specified API version is advertised and deprecated by the configured action.
        /// </summary>
        /// <param name="apiVersion">The advertised, but deprecated <see cref="ApiVersion">API version</see> not directly implemented by the action.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder AdvertisesDeprecatedApiVersion( ApiVersion apiVersion )
        {
            DeprecatedAdvertisedVersions.Add( apiVersion );
            return this;
        }

        void IDeclareApiVersionConventionBuilder.IsApiVersionNeutral() => IsApiVersionNeutral();

        void IDeclareApiVersionConventionBuilder.HasApiVersion( ApiVersion apiVersion ) => HasApiVersion( apiVersion );

        void IDeclareApiVersionConventionBuilder.HasDeprecatedApiVersion( ApiVersion apiVersion ) => HasDeprecatedApiVersion( apiVersion );

        void IDeclareApiVersionConventionBuilder.AdvertisesApiVersion( ApiVersion apiVersion ) => AdvertisesApiVersion( apiVersion );

        void IDeclareApiVersionConventionBuilder.AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) => AdvertisesDeprecatedApiVersion( apiVersion );

        void IMapToApiVersionConventionBuilder.MapToApiVersion( ApiVersion apiVersion ) => MapToApiVersion( apiVersion );

        IActionConventionBuilder IActionConventionBuilder.Action( MethodInfo actionMethod ) => Action( actionMethod );
    }
}