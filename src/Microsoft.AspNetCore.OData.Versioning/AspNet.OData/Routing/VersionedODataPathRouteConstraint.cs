namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using System;
    using static Microsoft.AspNetCore.Routing.RouteDirection;

    /// <summary>
    /// Represents an <see cref="ODataPathRouteConstraint">OData path route constraint</see> which supports versioning.
    /// </summary>
    [CLSCompliant( false )]
    public class VersionedODataPathRouteConstraint : ODataPathRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionedODataPathRouteConstraint" /> class.
        /// </summary>
        /// <param name="routeName">The name of the route this constraint is associated with.</param>
        /// <param name="apiVersion">The <see cref="ApiVersion">API version</see> associated with the route constraint.</param>
        public VersionedODataPathRouteConstraint( string routeName, ApiVersion apiVersion )
            : base( routeName ) => ApiVersion = apiVersion;

        /// <summary>
        /// Gets the API version matched by the current OData path route constraint.
        /// </summary>
        /// <value>The <see cref="ApiVersion">API version</see> associated with the route constraint.</value>
        public ApiVersion ApiVersion { get; }

        /// <summary>
        /// Determines whether the route constraint matches the specified criteria.
        /// </summary>
        /// <param name="httpContext">The current <see cref="HttpContext">HTTP context</see>.</param>
        /// <param name="route">The current <see cref="IRouter">route</see>.</param>
        /// <param name="routeKey">The key of the route parameter to match.</param>
        /// <param name="values">The current <see cref="RouteValueDictionary">collection</see> of route values.</param>
        /// <param name="routeDirection">The <see cref="RouteDirection">route direction</see> to match.</param>
        /// <returns>True if the route constraint is matched; otherwise, false.</returns>
        public override bool Match( HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection )
        {
            if ( httpContext == null )
            {
                throw new ArgumentNullException( nameof( httpContext ) );
            }

            if ( routeDirection == UrlGeneration || !TryGetRequestedApiVersion( httpContext, out var requestedVersion ) )
            {
                // note: if an error occurs reading the api version, still let the base constraint
                // match the request. the IActionSelector will produce 400 during action selection.
                return base.Match( httpContext, route, routeKey, values, routeDirection );
            }

            bool matched;

            try
            {
                matched = base.Match( httpContext, route, routeKey, values, routeDirection );
            }
            catch ( InvalidOperationException )
            {
                // note: the base implementation of Match will setup the container. if this happens more
                // than once, an exception is thrown. this most often occurs when policy allows implicitly
                // matching an api version and all routes must be visited to determine their candidacy. if
                // this happens, delete the container and retry.
                httpContext.Request.DeleteRequestContainer( true );
                matched = base.Match( httpContext, route, routeKey, values, routeDirection );
            }

            if ( !matched )
            {
                return false;
            }

            if ( requestedVersion == null )
            {
                // we definitely matched the route, but not necessarily the api version so
                // track this route as a matching candidate
                httpContext.ODataVersioningFeature().MatchingRoutes[ApiVersion] = RouteName;
                return false;
            }

            return ApiVersion == requestedVersion;
        }

        static bool TryGetRequestedApiVersion( HttpContext httpContext, out ApiVersion? apiVersion )
        {
            var feature = httpContext.Features.Get<IApiVersioningFeature>();

            try
            {
                apiVersion = feature.RequestedApiVersion;
            }
            catch ( AmbiguousApiVersionException )
            {
                apiVersion = default;
                return false;
            }

            return true;
        }
    }
}