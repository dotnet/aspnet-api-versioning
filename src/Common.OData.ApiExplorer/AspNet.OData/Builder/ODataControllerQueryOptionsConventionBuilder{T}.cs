namespace Microsoft.AspNet.OData.Builder
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using static System.ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Represents an OData query options convention builder.
    /// </summary>
#pragma warning disable SA1619 // Generic type parameters should be documented partial class; false positive
    public partial class ODataControllerQueryOptionsConventionBuilder<T> : IODataQueryOptionsConventionBuilder, IODataActionQueryOptionsConventionBuilder<T>
#pragma warning restore SA1619
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataControllerQueryOptionsConventionBuilder{T}"/> class.
        /// </summary>
        public ODataControllerQueryOptionsConventionBuilder() => ActionBuilders = new ODataActionQueryOptionsConventionBuilderCollection<T>( this );

        /// <summary>
        /// Gets a collection of controller action convention builders.
        /// </summary>
        /// <value>A <see cref="ODataActionQueryOptionsConventionBuilderCollection{T}">collection</see> of
        /// <see cref="ODataActionQueryOptionsConventionBuilder{T}">controller action convention builders</see>.</value>
        protected virtual ODataActionQueryOptionsConventionBuilderCollection<T> ActionBuilders { get; }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller action method.
        /// </summary>
        /// <param name="actionMethod">The <see cref="MethodInfo">method</see> representing the controller action.</param>
        /// <returns>A new or existing <see cref="ODataActionQueryOptionsConventionBuilder{T}"/>.</returns>
        [EditorBrowsable( Never )]
        public virtual ODataActionQueryOptionsConventionBuilder<T> Action( MethodInfo actionMethod ) => ActionBuilders.GetOrAdd( actionMethod );

        /// <inheritdoc />
        public virtual IODataQueryOptionsConvention Build( ODataQueryOptionSettings settings ) =>
            new ODataControllerQueryOptionConvention( Lookup, settings );

        bool Lookup( MethodInfo action, ODataQueryOptionSettings settings, out IODataQueryOptionsConvention? convention )
        {
            if ( ActionBuilders.TryGetValue( action, out var builder ) )
            {
                convention = builder!.Build( settings );
                return true;
            }

            convention = default;
            return false;
        }
    }
}