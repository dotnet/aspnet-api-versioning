namespace Microsoft.Web.Http.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static System.Web.Http.Routing.HttpRouteDirection;
    using static ApiVersion;

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
            if ( routeDirection != UriResolution )
            {
                return false;
            }

            var value = default( string );
            var requestedVersion = default( ApiVersion );

            if ( !values.TryGetValue( parameterName, out value ) || !TryParse( value, out requestedVersion ) )
            {
                return false;
            }

            requestedVersion = DefaultMinorVersionToZeroWhenOnlyMajorVersionIsSpecified( requestedVersion );
            request.SetRequestedApiVersion( requestedVersion );
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
