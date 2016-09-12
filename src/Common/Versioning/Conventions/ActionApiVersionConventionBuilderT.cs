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
    /// Represents a builder for API versions applied to a controller action.
    /// </summary>
    public partial class ActionApiVersionConventionBuilder<T>
    {
        private readonly HashSet<ApiVersion> mappedVersions = new HashSet<ApiVersion>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilder{T}"/> class.
        /// </summary>
        /// <param name="controllerBuilder">The <see cref="ControllerApiVersionConventionBuilder{T}">controller builder</see>
        /// the action builder belongs to.</param>
        public ActionApiVersionConventionBuilder( ControllerApiVersionConventionBuilder<T> controllerBuilder )
        {
            Arg.NotNull( controllerBuilder, nameof( controllerBuilder ) );
            ControllerBuilder = controllerBuilder;
        }

        /// <summary>
        /// Gets the controller builder the action builder belongs to.
        /// </summary>
        /// <value>The associated <see cref="ControllerApiVersionConventionBuilder{T}"/>.</value>
        protected ControllerApiVersionConventionBuilder<T> ControllerBuilder { get; }

        /// <summary>
        /// Gets the collection of API versions mapped to the current action.
        /// </summary>
        /// <value>A <see cref="ICollection{T}">collection</see> of mapped <see cref="ApiVersion">API versions</see>.</value>
        protected ICollection<ApiVersion> MappedVersions => mappedVersions;

        /// <summary>
        /// Maps the specified API version to the configured controller action.
        /// </summary>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> to map to the action.</param>
        /// <returns>The original <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        public virtual ActionApiVersionConventionBuilder<T> MapToApiVersion( ApiVersion apiVersion )
        {
            Arg.NotNull( apiVersion, nameof( apiVersion ) );
            Contract.Ensures( Contract.Result<ActionApiVersionConventionBuilder<T>>() != null );

            mappedVersions.Add( apiVersion );
            return this;
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}"/>.</returns>
        [EditorBrowsable( Never )]
        public virtual ActionApiVersionConventionBuilder<T> Action( MethodInfo actionMethod ) => ControllerBuilder.Action( actionMethod );
    }
}