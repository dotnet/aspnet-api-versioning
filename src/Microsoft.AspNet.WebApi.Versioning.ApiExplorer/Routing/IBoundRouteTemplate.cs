namespace Microsoft.Web.Http.Routing
{
    using System;
    using System.Web.Http.Routing;

    /// <summary>
    /// Defines the behavior of a bound route template.
    /// </summary>
    public interface IBoundRouteTemplate
    {
        /// <summary>
        /// Gets or sets the build template.
        /// </summary>
        /// <value>The bound template.</value>
        string BoundTemplate { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        /// <summary>
        /// Gets or sets the template parameter values.
        /// </summary>
        /// <value>The template <see cref="HttpRouteValueDictionary">route value dictionary</see>.</value>
        HttpRouteValueDictionary Values { get; set; }
#pragma warning restore CA2227
    }
}