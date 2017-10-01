namespace Microsoft.Web.Http.Versioning.Conventions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Web.Http.Controllers;

    /// <summary>
    /// Represents an object used to configure and create API version conventions for a controllers and their actions.
    /// </summary>
    public class ApiVersionConventionBuilder
    {
        /// <summary>
        /// Gets a collection of controller conventions.
        /// </summary>
        /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of controller <see cref="IApiVersionConvention{T}">API version conventions</see>.</value>
        protected IDictionary<Type, IApiVersionConvention<HttpControllerDescriptor>> ControllerConventions { get; } =
            new Dictionary<Type, IApiVersionConvention<HttpControllerDescriptor>>();

        /// <summary>
        /// Gets the count of configured conventions.
        /// </summary>
        /// <value>The total count of configured conventions.</value>
        public virtual int Count => ControllerConventions.Count;

        /// <summary>
        /// Gets or creates the convention builder for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The <see cref="Type">type</see> of controller to build conventions for.</typeparam>
        /// <returns>A new or existing <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder<TController> Controller<TController>() where TController : IHttpController
        {
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder<TController>>() != null );

            var key = typeof( TController );

            if ( !ControllerConventions.TryGetValue( key, out var convention ) )
            {
                var typedConvention = new ControllerApiVersionConventionBuilder<TController>();
                ControllerConventions[key] = typedConvention;
                return typedConvention;
            }

            if ( convention is ControllerApiVersionConventionBuilder<TController> builder )
            {
                return builder;
            }

            throw new InvalidOperationException( SR.ConventionStyleMismatch.FormatDefault( key.Name ) );
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller.
        /// </summary>
        /// <param name="controllerType">The <see cref="Type">type</see> of controller to build conventions for.</param>
        /// <returns>A new or existing <see cref="ControllerApiVersionConventionBuilder"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder Controller( Type controllerType )
        {
            Arg.NotNull( controllerType, nameof( controllerType ) );
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder>() != null );

            var key = controllerType;

            if ( !ControllerConventions.TryGetValue( key, out var convention ) )
            {
                var typedConvention = new ControllerApiVersionConventionBuilder( controllerType );
                ControllerConventions[key] = typedConvention;
                return typedConvention;
            }

            if ( convention is ControllerApiVersionConventionBuilder builder )
            {
                return builder;
            }

            throw new InvalidOperationException( SR.ConventionStyleMismatch.FormatDefault( key.Name ) );
        }

        /// <summary>
        /// Applies the defined API version conventions to the specified controller.
        /// </summary>
        /// <param name="controllerDescriptor">The <see cref="HttpControllerDescriptor">controller descriptor</see>
        /// to apply configured conventions to.</param>
        /// <returns>True if any conventions were applied to the
        /// <paramref name="controllerDescriptor">controller descriptor</paramref>; otherwise, false.</returns>
        public virtual bool ApplyTo( HttpControllerDescriptor controllerDescriptor )
        {
            Arg.NotNull( controllerDescriptor, nameof( controllerDescriptor ) );

            var key = controllerDescriptor.ControllerType;

            if ( ControllerConventions.TryGetValue( key, out var convention ) )
            {
                convention.ApplyTo( controllerDescriptor );
                return true;
            }

            return false;
        }
    }
}