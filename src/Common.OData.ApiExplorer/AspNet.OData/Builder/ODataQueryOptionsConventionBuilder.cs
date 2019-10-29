namespace Microsoft.AspNet.OData.Builder
{
#if !WEBAPI
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
#endif
    using System;
    using System.Collections.Generic;
#if WEBAPI
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
    using TypeInfo = System.Type;
#else
    using System.Reflection;
#endif

    /// <summary>
    /// Represents an OData query options convention builder.
    /// </summary>
    public partial class ODataQueryOptionsConventionBuilder
    {
        /// <summary>
        /// Gets or sets the provider used to describe query options.
        /// </summary>
        /// <value>The <see cref="IODataQueryOptionDescriptionProvider">provider</see> used to describe OData query options.</value>
        public IODataQueryOptionDescriptionProvider DescriptionProvider { get; set; } = new DefaultODataQueryOptionDescriptionProvider();

        /// <summary>
        /// Gets the count of configured conventions.
        /// </summary>
        /// <value>The total count of configured conventions.</value>
        public virtual int Count => ConventionBuilders.Count + Conventions.Count;

        /// <summary>
        /// Gets a collection of controller convention builders.
        /// </summary>
        /// <value>A <see cref="IDictionary{TKey, TValue}">collection</see> of <see cref="IODataQueryOptionsConventionBuilder">controller convention builders</see>.</value>
        protected IDictionary<TypeInfo, IODataQueryOptionsConventionBuilder> ConventionBuilders { get; } = new Dictionary<TypeInfo, IODataQueryOptionsConventionBuilder>();

        /// <summary>
        /// Gets a collection of OData query option conventions.
        /// </summary>
        /// <value>A <see cref="IList{T}">list</see> of <see cref="IODataQueryOptionsConvention">OData query option conventions</see>.</value>
        protected IList<IODataQueryOptionsConvention> Conventions { get; } = new List<IODataQueryOptionsConvention>();

        /// <summary>
        /// Gets or creates the convention builder for the specified controller.
        /// </summary>
        /// <typeparam name="TController">The <see cref="Type">type</see> of controller to build conventions for.</typeparam>
        /// <returns>A new or existing <see cref="ODataControllerQueryOptionsConventionBuilder{T}"/>.</returns>
        public virtual ODataControllerQueryOptionsConventionBuilder<TController> Controller<TController>()
            where TController : notnull
#if WEBAPI
#pragma warning disable SA1001 // Commas should be spaced correctly
       , IHttpController
#pragma warning restore SA1001 // Commas should be spaced correctly
#endif
        {
            var key = GetKey( typeof( TController ) );

            if ( !ConventionBuilders.TryGetValue( key, out var builder ) )
            {
                var newBuilder = new ODataControllerQueryOptionsConventionBuilder<TController>();
                ConventionBuilders[key] = newBuilder;
                return newBuilder;
            }

            if ( builder is ODataControllerQueryOptionsConventionBuilder<TController> typedBuilder )
            {
                return typedBuilder;
            }

            throw new InvalidOperationException( SR.ConventionStyleMismatch.FormatDefault( key.Name ) );
        }

        /// <summary>
        /// Gets or creates the convention builder for the specified controller.
        /// </summary>
        /// <param name="controllerType">The <see cref="Type">type</see> of controller to build conventions for.</param>
        /// <returns>A new or existing <see cref="ODataControllerQueryOptionsConventionBuilder"/>.</returns>
        public virtual ODataControllerQueryOptionsConventionBuilder Controller( Type controllerType )
        {
            if ( controllerType == null )
            {
                throw new ArgumentNullException( nameof( controllerType ) );
            }

            var key = GetKey( controllerType );

            if ( !ConventionBuilders.TryGetValue( key, out var builder ) )
            {
                var newBuilder = new ODataControllerQueryOptionsConventionBuilder( controllerType );
                ConventionBuilders[key] = newBuilder;
                return newBuilder;
            }

            if ( builder is ODataControllerQueryOptionsConventionBuilder typedBuilder )
            {
                return typedBuilder;
            }

            throw new InvalidOperationException( SR.ConventionStyleMismatch.FormatDefault( key.Name ) );
        }

        /// <summary>
        /// Adds a new OData query option convention.
        /// </summary>
        /// <param name="convention">The <see cref="IODataQueryOptionsConvention">convention</see> to be applied.</param>
        public virtual void Add( IODataQueryOptionsConvention convention ) => Conventions.Add( convention );

        /// <summary>
        /// Applies the defined OData query option conventions to the specified API description.
        /// </summary>
        /// <param name="apiDescriptions">The <see cref="IEnumerable{T}">sequence</see> of <see cref="ApiDescription">API descriptions</see>
        /// to apply configured conventions to.</param>
        /// <param name="queryOptionSettings">The <see cref="ODataQueryOptionSettings">settings</see> used to apply OData query option conventions.</param>
        public virtual void ApplyTo( IEnumerable<ApiDescription> apiDescriptions, ODataQueryOptionSettings queryOptionSettings )
        {
            if ( apiDescriptions == null )
            {
                throw new ArgumentNullException( nameof( apiDescriptions ) );
            }

            var conventions = new Dictionary<TypeInfo, IODataQueryOptionsConvention>();

            foreach ( var description in apiDescriptions )
            {
                var controller = GetController( description );

                if ( !conventions.TryGetValue( controller, out var convention ) )
                {
                    if ( !controller.IsODataController() && !IsODataLike( description ) )
                    {
                        continue;
                    }

                    if ( !ConventionBuilders.TryGetValue( controller, out var builder ) )
                    {
                        builder = new ODataControllerQueryOptionsConventionBuilder( controller );
                    }

                    convention = builder.Build( queryOptionSettings );
                    conventions.Add( controller, convention );
                }

                convention.ApplyTo( description );

                for ( var i = 0; i < Conventions.Count; i++ )
                {
                    Conventions[i].ApplyTo( description );
                }
            }
        }
    }
}