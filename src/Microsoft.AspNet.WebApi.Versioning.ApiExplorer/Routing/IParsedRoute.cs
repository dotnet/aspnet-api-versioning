namespace Microsoft.Web.Http.Routing
{
    using System.Collections.Generic;
    using System.Web.Http.Routing;

    /// <summary>
    /// Defines the behavior of a parsed route.
    /// </summary>
    public interface IParsedRoute
    {
        /// <summary>
        /// Binds the route using the specified values and constraints.
        /// </summary>
        /// <param name="currentValues">The current <see cref="IDictionary{TKey, TValue}">collection</see> of values.</param>
        /// <param name="values">The current <see cref="IDictionary{TKey, TValue}">collection</see> to bind.</param>
        /// <param name="defaultValues">The <see cref="HttpRouteValueDictionary">dictionary</see> of default values.</param>
        /// <param name="constraints">The <see cref="HttpRouteValueDictionary">dictionary</see> of constraints.</param>
        /// <returns>A new <see cref="IBoundRouteTemplate">bound route template</see>.</returns>
        IBoundRouteTemplate Bind( IDictionary<string, object>? currentValues, IDictionary<string, object> values, HttpRouteValueDictionary defaultValues, HttpRouteValueDictionary constraints );

        /// <summary>
        /// Gets the path segments associated with the parsed route.
        /// </summary>
        /// <value>A <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="IPathSegment">path segments</see>.</value>
        IReadOnlyList<IPathSegment> PathSegments { get; }
    }
}