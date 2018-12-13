namespace Microsoft.AspNet.OData.Builder
{
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public static partial class ODataActionQueryOptionsConventionBuilderExtensions
    {
        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <typeparam name="TModel">The type of <see cref="ICommonModel">model</see>.</typeparam>
        /// <param name="builder">The extended convention builder.</param>
        /// <param name="maxNodeCount">The maximum number of expressions in the $orderby query option or zero to indicate the default.</param>
        /// <param name="properties">The array of property names that can appear in the $orderby query option.
        /// An empty array indicates that any property can appear in the $orderby query option.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static ODataActionQueryOptionsConventionBuilder<TModel> AllowOrderBy<TModel>(
            this ODataActionQueryOptionsConventionBuilder<TModel> builder, int maxNodeCount, params string[] properties )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( properties, nameof( properties ) );
            Arg.GreaterThanOrEqualTo( maxNodeCount, 0, nameof( maxNodeCount ) );

            return builder.AllowOrderBy( maxNodeCount, properties.AsEnumerable() );
        }

        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <typeparam name="TModel">The type of <see cref="ICommonModel">model</see>.</typeparam>
        /// <param name="builder">The extended convention builder.</param>
        /// <param name="properties">The <see cref="IEnumerable{T}">sequence</see> of property names that can appear in the $orderby query option.
        /// An empty sequence indicates that any property can appear in the $orderby query option.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static ODataActionQueryOptionsConventionBuilder<TModel> AllowOrderBy<TModel>(
            this ODataActionQueryOptionsConventionBuilder<TModel> builder, IEnumerable<string> properties )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( properties, nameof( properties ) );

            return builder.AllowOrderBy( default, properties );
        }

        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <typeparam name="TModel">The type of <see cref="ICommonModel">model</see>.</typeparam>
        /// <param name="builder">The extended convention builder.</param>
        /// <param name="properties">The array of property names that can appear in the $orderby query option.
        /// An empty array indicates that any property can appear in the $orderby query option.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static ODataActionQueryOptionsConventionBuilder<TModel> AllowOrderBy<TModel>(
            this ODataActionQueryOptionsConventionBuilder<TModel> builder, params string[] properties )
        {
            Arg.NotNull( builder, nameof( builder ) );
            Arg.NotNull( properties, nameof( properties ) );

            return builder.AllowOrderBy( default, properties.AsEnumerable() );
        }
    }
}