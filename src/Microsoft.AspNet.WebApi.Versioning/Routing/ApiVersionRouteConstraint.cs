namespace Microsoft.Web.Http.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static ApiVersion;
    using static System.String;
    using static System.Web.Http.Routing.HttpRouteDirection;

    /// <summary>
    /// Represents a route constraint for <see cref="ApiVersion">API versions</see>.
    /// </summary>
    public sealed class ApiVersionRouteConstraint : IHttpRouteConstraint
    {
        /// <summary>
        /// Determines whether the route constraint matches the specified criteria.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
        /// <param name="route">The current <see cref="IHttpRoute">route</see>.</param>
        /// <param name="parameterName">The parameter name to match.</param>
        /// <param name="values">The current <see cref="IDictionary{TKey, TValue}">collection</see> of route values.</param>
        /// <param name="routeDirection">The <see cref="HttpRouteDirection">route direction</see> to match.</param>
        /// <returns>True if the route constraint is matched; otherwise, false.</returns>
        public bool Match( HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection )
        {
            var value = default( string );

            if ( routeDirection == UriGeneration )
            {
                return !IsNullOrEmpty( parameterName ) && values.TryGetValue( parameterName, out value ) && !IsNullOrEmpty( value );
            }

            var requestedVersion = default( ApiVersion );

            if ( !values.TryGetValue( parameterName, out value ) || !TryParse( value, out requestedVersion ) )
            {
                return false;
            }

            request.SetRequestedApiVersion( requestedVersion );
            return true;
        }
    }
}
