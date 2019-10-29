namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.OData;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static System.Net.HttpStatusCode;
    using static System.Web.Http.Routing.HttpRouteDirection;

    /// <summary>
    /// Represents an <see cref="ODataPathRouteConstraint">OData path route constraint</see> which supports versioning.
    /// </summary>
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
        /// Determines whether this instance equals a specified route.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="route">The route to compare.</param>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="values">A list of parameter values.</param>
        /// <param name="routeDirection">The route direction.</param>
        /// <returns>True if this instance equals a specified route; otherwise, false.</returns>
        public override bool Match( HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection )
        {
            if ( values == null )
            {
                throw new ArgumentNullException( nameof( values ) );
            }

            if ( routeDirection == UriGeneration )
            {
                return base.Match( request, route, parameterName, values, routeDirection );
            }

            var requestedVersion = GetRequestedApiVersionOrReturnBadRequest( request );
            bool matched;

            try
            {
                matched = base.Match( request, route, parameterName, values, routeDirection );
            }
            catch ( InvalidOperationException )
            {
                // note: the base implementation of Match will setup the container. if this happens more
                // than once, an exception is thrown. this most often occurs when policy allows implicitly
                // matching an api version and all routes must be visited to determine their candidacy. if
                // this happens, delete the container and retry.
                request.DeleteRequestContainer( true );
                matched = base.Match( request, route, parameterName, values, routeDirection );
            }

            if ( !matched )
            {
                return false;
            }

            if ( requestedVersion == null )
            {
                // we definitely matched the route, but not necessarily the api version so
                // track this route as a matching candidate
                request.ODataApiVersionProperties().MatchingRoutes[ApiVersion] = RouteName;
                return false;
            }

            if ( ApiVersion == requestedVersion )
            {
                DecorateUrlHelperWithApiVersionRouteValueIfNecessary( request, values );
                return true;
            }

            return false;
        }

        static ApiVersion? GetRequestedApiVersionOrReturnBadRequest( HttpRequestMessage request )
        {
            var properties = request.ApiVersionProperties();

            try
            {
                return properties.RequestedApiVersion;
            }
            catch ( AmbiguousApiVersionException ex )
            {
                var error = new ODataError() { ErrorCode = "AmbiguousApiVersion", Message = ex.Message };
                throw new HttpResponseException( request.CreateResponse( BadRequest, error ) );
            }
        }

        static void DecorateUrlHelperWithApiVersionRouteValueIfNecessary( HttpRequestMessage request, IDictionary<string, object> values )
        {
            object apiVersion;
            string routeConstraintName;
            var configuration = request.GetConfiguration();

            if ( configuration == null )
            {
                routeConstraintName = nameof( apiVersion );
            }
            else
            {
                routeConstraintName = configuration.GetApiVersioningOptions().RouteConstraintName;
            }

            if ( !values.TryGetValue( routeConstraintName, out apiVersion ) )
            {
                return;
            }

            var requestContext = request.GetRequestContext();

            if ( !( requestContext.Url is VersionedUrlHelperDecorator ) )
            {
                requestContext.Url = new VersionedUrlHelperDecorator( requestContext.Url, routeConstraintName, apiVersion );
            }
        }
    }
}