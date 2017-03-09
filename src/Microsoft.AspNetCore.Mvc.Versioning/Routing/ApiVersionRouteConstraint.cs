namespace Microsoft.AspNetCore.Mvc.Routing
{
    using AspNetCore.Routing;
    using Http;
    using System;
    using static ApiVersion;
    using static AspNetCore.Routing.RouteDirection;
    using static System.String;

    /// <summary>
    /// Represents a route constraint for service <see cref="ApiVersion">API versions</see>.
    /// </summary>
    [CLSCompliant( false )]
    public sealed class ApiVersionRouteConstraint : IRouteConstraint
    {
        /// <summary>
        /// Determines whether the route constraint matches the specified criteria.
        /// </summary>
        /// <param name="httpContext">The current <see cref="HttpContext">HTTP context</see>.</param>
        /// <param name="route">The current <see cref="IRouter">route</see>.</param>
        /// <param name="routeKey">The key of the route parameter to match.</param>
        /// <param name="values">The current <see cref="RouteValueDictionary">collection</see> of route values.</param>
        /// <param name="routeDirection">The <see cref="RouteDirection">route direction</see> to match.</param>
        /// <returns>True if the route constraint is matched; otherwise, false.</returns>
        public bool Match( HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection )
        {
            if ( IsNullOrEmpty( routeKey ) )
            {
                return false;
            }

            var properties = httpContext.ApiVersionProperties();

            if ( values.TryGetValue( routeKey, out string value ) )
            {
                properties.RouteParameterName = routeKey;
            }
            else
            {
                return false;
            }

            if ( routeDirection == UrlGeneration )
            {
                return !IsNullOrEmpty( value );
            }

            if ( TryParse( value, out var requestedVersion ) )
            {
                properties.ApiVersion = requestedVersion;
                return true;
            }

            return false;
        }
    }
}