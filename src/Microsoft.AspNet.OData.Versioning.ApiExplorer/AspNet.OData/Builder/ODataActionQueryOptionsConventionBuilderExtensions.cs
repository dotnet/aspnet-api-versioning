namespace Microsoft.AspNet.OData.Builder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http.Controllers;

    /// <content>
    /// Provides additional implementation specific to Microsoft ASP.NET Web API.
    /// </content>
    public static partial class ODataActionQueryOptionsConventionBuilderExtensions
    {
        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <typeparam name="TController">The type of <see cref="IHttpController">controller</see>.</typeparam>
        /// <param name="builder">The extended convention builder.</param>
        /// <param name="maxNodeCount">The maximum number of expressions in the $orderby query option or zero to indicate the default.</param>
        /// <param name="properties">The array of property names that can appear in the $orderby query option.
        /// An empty array indicates that any property can appear in the $orderby query option.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static ODataActionQueryOptionsConventionBuilder<TController> AllowOrderBy<TController>(
            this ODataActionQueryOptionsConventionBuilder<TController> builder,
            int maxNodeCount,
            params string[] properties )
            where TController : notnull, IHttpController
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            return builder.AllowOrderBy( maxNodeCount, properties.AsEnumerable() );
        }

        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <typeparam name="TController">The type of <see cref="IHttpController">controller</see>.</typeparam>
        /// <param name="builder">The extended convention builder.</param>
        /// <param name="properties">The <see cref="IEnumerable{T}">sequence</see> of property names that can appear in the $orderby query option.
        /// An empty sequence indicates that any property can appear in the $orderby query option.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static ODataActionQueryOptionsConventionBuilder<TController> AllowOrderBy<TController>(
            this ODataActionQueryOptionsConventionBuilder<TController> builder,
            IEnumerable<string> properties )
            where TController : notnull, IHttpController
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            return builder.AllowOrderBy( default, properties );
        }

        /// <summary>
        /// Allows the $orderby query option.
        /// </summary>
        /// <typeparam name="TController">The type of <see cref="IHttpController">controller</see>.</typeparam>
        /// <param name="builder">The extended convention builder.</param>
        /// <param name="properties">The array of property names that can appear in the $orderby query option.
        /// An empty array indicates that any property can appear in the $orderby query option.</param>
        /// <returns>The original <paramref name="builder"/>.</returns>
        public static ODataActionQueryOptionsConventionBuilder<TController> AllowOrderBy<TController>(
            this ODataActionQueryOptionsConventionBuilder<TController> builder,
            params string[] properties )
            where TController : notnull, IHttpController
        {
            if ( builder == null )
            {
                throw new ArgumentNullException( nameof( builder ) );
            }

            return builder.AllowOrderBy( default, properties.AsEnumerable() );
        }
    }
}