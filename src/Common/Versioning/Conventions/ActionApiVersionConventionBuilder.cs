#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    /// Represents a builder for API versions applied to a controller action.
    /// </summary>
    public partial class ActionApiVersionConventionBuilder : ActionApiVersionConventionBuilderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilder"/> class.
        /// </summary>
        /// <param name="controllerBuilder">The <see cref="ControllerApiVersionConventionBuilder">controller builder</see>
        /// the action builder belongs to.</param>
        public ActionApiVersionConventionBuilder( ControllerApiVersionConventionBuilder controllerBuilder )
        {
            Arg.NotNull( controllerBuilder, nameof( controllerBuilder ) );
            ControllerBuilder = controllerBuilder;
        }

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
        /// Maps the specified API version to the configured controller action.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to map to the action.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder MapToApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder>() != null );

            MappedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder"/>.</returns>
        public virtual ActionApiVersionConventionBuilder Action( MethodInfo actionMethod ) => ControllerBuilder.Action( actionMethod );
    }
}