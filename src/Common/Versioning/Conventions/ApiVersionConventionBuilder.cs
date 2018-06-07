#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
#if WEBAPI
    using System.Web.Http.Controllers;
    using Model = System.Web.Http.Controllers.HttpControllerDescriptor;
    using TypeInfo = System.Type;
#else
    using System.Reflection;
    using Model = Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel;
#endif

    /// <summary>
    /// Represents an object used to configure and create API version conventions for a controllers and their actions.
    /// </summary>
    public partial class ApiVersionConventionBuilder
    {
        /// <summary>
        /// Gets a collection of controller convention builders.
        /// </summary>
        /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of <see cref="IControllerConventionBuilder">controller convention builders</see>.</value>
        protected IDictionary<TypeInfo, IControllerConventionBuilder> ControllerConventionBuilders { get; } = new Dictionary<TypeInfo, IControllerConventionBuilder>();

        /// <summary>
        /// Gets a collection of controller conventions.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of <see cref="IControllerConvention">controller conventions</see>.</value>
        protected IList<IControllerConvention> ControllerConventions { get; } = new List<IControllerConvention>();

        /// <summary>
        /// Gets the count of configured conventions.
        /// </summary>
        /// <value>The total count of configured conventions.</value>
        public virtual int Count => ControllerConventionBuilders.Count + ControllerConventions.Count;

        /// <summary>
        /// Gets or creates the convention builder for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The <see cref="Type">type</see> of controller to build conventions for.</typeparam>
        /// <returns>A new or existing <see cref="ControllerApiVersionConventionBuilder{T}"/>.</returns>
        public virtual ControllerApiVersionConventionBuilder<TController> Controller<TController>()
#if WEBAPI
            where TController : IHttpController
#endif
        {
            Contract.Ensures( Contract.Result<ControllerApiVersionConventionBuilder<TController>>() != null );

            var key = GetKey( typeof( TController ) );

            if ( !ControllerConventionBuilders.TryGetValue( key, out var builder ) )
            {
                var newBuilder = new ControllerApiVersionConventionBuilder<TController>();
                ControllerConventionBuilders[key] = newBuilder;
                return newBuilder;
            }

            if ( builder is ControllerApiVersionConventionBuilder<TController> typedBuilder )
            {
                return typedBuilder;
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

            var key = GetKey( controllerType );

            if ( !ControllerConventionBuilders.TryGetValue( key, out var builder ) )
            {
                var newBuilder = new ControllerApiVersionConventionBuilder( controllerType );
                ControllerConventionBuilders[key] = newBuilder;
                return newBuilder;
            }

            if ( builder is ControllerApiVersionConventionBuilder typedBuilder )
            {
                return typedBuilder;
            }

            throw new InvalidOperationException( SR.ConventionStyleMismatch.FormatDefault( key.Name ) );
        }

        /// <summary>
        /// Adds a new convention applied to all controllers.
        /// </summary>
        /// <param name="convention">The <see cref="IControllerConvention">convention</see> to be applied.</param>
        public virtual void Add( IControllerConvention convention )
        {
            Arg.NotNull( convention, nameof( convention ) );
            ControllerConventions.Add( convention );
        }

        bool InternalApplyTo( Model model )
        {
            var key = model.ControllerType;
            var hasExplicitConventions = ControllerConventionBuilders.TryGetValue( key, out var builder );
            var applied = hasExplicitConventions;

            if ( !hasExplicitConventions )
            {
                var hasNoImplicitConventions = ControllerConventions.Count == 0;

                if ( hasNoImplicitConventions )
                {
                    return false;
                }

                builder = new ControllerApiVersionConventionBuilder( model.ControllerType );
            }

            foreach ( var convention in ControllerConventions )
            {
                applied |= convention.Apply( builder, model );
            }

            if ( applied )
            {
                builder.ApplyTo( model );
            }

            return applied;
        }
    }
}