#if WEBAPI
namespace Microsoft.Web.Http.Versioning.Conventions
#else
namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
#endif
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents a collection of controller action convention builders.
    /// </summary>
#pragma warning disable SA1619 // Generic type parameters should be documented partial class; false positive
    public partial class ActionApiVersionConventionBuilderCollection<T> : IReadOnlyCollection<ActionApiVersionConventionBuilder<T>>
#pragma warning restore SA1619
    {
        readonly ControllerApiVersionConventionBuilder<T> controllerBuilder;
        readonly IList<ActionBuilderMapping<T>> actionBuilderMappings = new List<ActionBuilderMapping<T>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionApiVersionConventionBuilderCollection{T}"/> class.
        /// </summary>
        /// <param name="controllerBuilder">The associated <see cref="ControllerApiVersionConventionBuilder{T}">controller convention builder</see>.</param>
        public ActionApiVersionConventionBuilderCollection( ControllerApiVersionConventionBuilder<T> controllerBuilder )
        {
            Arg.NotNull( controllerBuilder, nameof( controllerBuilder ) );
            this.controllerBuilder = controllerBuilder;
        }

        /// <summary>
        /// Gets or adds a controller action convention builder for the specified method.
        /// </summary>
        /// <param name="actionMethod">The controller action method to get or add the convention builder for.</param>
        /// <returns>A new or existing <see cref="ActionApiVersionConventionBuilder{T}">controller action convention builder</see>.</returns>
        protected internal virtual ActionApiVersionConventionBuilder<T> GetOrAdd( MethodInfo actionMethod )
        {
            Arg.NotNull( actionMethod, nameof( actionMethod ) );

            var mapping = actionBuilderMappings.FirstOrDefault( m => m.Method == actionMethod );

            if ( mapping == null )
            {
                mapping = new ActionBuilderMapping<T>( actionMethod, new ActionApiVersionConventionBuilder<T>( controllerBuilder ) );
                actionBuilderMappings.Add( mapping );
            }

            return mapping.Builder;
        }

        /// <summary>
        /// Gets a count of the controller action convention builders in the collection.
        /// </summary>
        /// <value>The total number of controller action convention builders in the collection.</value>
        public virtual int Count => actionBuilderMappings.Count;

        /// <summary>
        /// Attempts to retrieve the controller action convention builder for the specified method.
        /// </summary>
        /// <param name="actionMethod">The controller action method to get the convention builder for.</param>
        /// <param name="actionBuilder">The <see cref="ActionApiVersionConventionBuilder{T}">controller action convention builder</see> or <c>null</c>.</param>
        /// <returns>True if the <paramref name="actionBuilder">action builder</paramref> is successfully retrieved; otherwise, false.</returns>
        public virtual bool TryGetValue( MethodInfo actionMethod, out ActionApiVersionConventionBuilder<T> actionBuilder )
        {
            actionBuilder = null;

            if ( actionMethod == null )
            {
                return false;
            }

            var mapping = actionBuilderMappings.FirstOrDefault( m => m.Method == actionMethod );

            return ( actionBuilder = mapping?.Builder ) != null;
        }

        /// <summary>
        /// Returns an iterator that enumerates the controller action convention builders in the collection.
        /// </summary>
        /// <returns>An <see cref="IEnumerator{T}"/> object.</returns>
        public virtual IEnumerator<ActionApiVersionConventionBuilder<T>> GetEnumerator()
        {
            foreach ( var mapping in actionBuilderMappings )
            {
                yield return mapping.Builder;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        sealed partial class ActionBuilderMapping<TModel>
        {
            internal ActionBuilderMapping( MethodInfo method, ActionApiVersionConventionBuilder<TModel> builder )
            {
                Contract.Requires( method != null );
                Contract.Requires( builder != null );

                Method = method;
                Builder = builder;
            }

            internal MethodInfo Method { get; }

            internal ActionApiVersionConventionBuilder<TModel> Builder { get; }
        }
    }
}