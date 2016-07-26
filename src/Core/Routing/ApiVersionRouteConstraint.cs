namespace Microsoft.AspNetCore.Mvc.Routing
{
    using AspNetCore.Routing;
    using Http;
    using System;
    using System.Diagnostics.Contracts;
    using static ApiVersion;
    using static AspNetCore.Routing.RouteDirection;

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
            if ( routeDirection != IncomingRequest )
            {
                return false;
            }

            var value = default( string );
            var requestedVersion = default( ApiVersion );

            if ( !values.TryGetValue( routeKey, out value ) || !TryParse( value, out requestedVersion ) )
            {
                return false;
            }

            requestedVersion = DefaultMinorVersionToZeroWhenOnlyMajorVersionIsSpecified( requestedVersion );
            httpContext.SetRequestedApiVersion( requestedVersion );
            return true;
        }

        private static ApiVersion DefaultMinorVersionToZeroWhenOnlyMajorVersionIsSpecified( ApiVersion requestedVersion )
        {
            Contract.Requires( requestedVersion != null );
            Contract.Ensures( Contract.Result<ApiVersion>() != null );

            if ( requestedVersion.MajorVersion == null )
            {
                return requestedVersion;
            }

            if ( requestedVersion.MinorVersion == null )
            {
                return new ApiVersion( requestedVersion.GroupVersion, requestedVersion.MajorVersion, new int?( 0 ), requestedVersion.Status );
            }

            return requestedVersion;
        }
    }
}
