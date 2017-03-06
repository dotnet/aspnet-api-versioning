namespace Microsoft.Web.Http.Versioning
{
    using Routing;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static System.String;

    /// <content>
    /// Provides the implementation for ASP.NET Web API.
    /// </content>
    public partial class UrlSegmentApiVersionReader
    {
        /// <summary>
        /// Reads the service API version value from a request.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">HTTP request</see> to read the API version from.</param>
        /// <returns>The raw, unparsed service API version value read from the request or <c>null</c> if request does not contain an API version.</returns>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public virtual string Read( HttpRequestMessage request )
        {
            Arg.NotNull( request, nameof( request ) );

            var routeData = request.GetRouteData();

            if ( routeData == null )
            {
                return null;
            }

            var key = request.ApiVersionProperties().RouteParameterName;
            var subRouteData = routeData.GetSubRoutes() ?? new[] { routeData };
            var value = default( object );

            if ( IsNullOrEmpty( key ) )
            {
                foreach ( var subRouteDatum in subRouteData )
                {
                    key = GetRouteParameterNameFromConstraintNameInTemplate( subRouteDatum );

                    if ( key != null && subRouteDatum.Values.TryGetValue( key, out value ) )
                    {
                        return value.ToString();
                    }
                }
            }
            else
            {
                foreach ( var subRouteDatum in subRouteData )
                {
                    if ( subRouteDatum.Values.TryGetValue( key, out value ) )
                    {
                        return value.ToString();
                    }
                }
            }

            return null;
        }

        static string GetRouteParameterNameFromConstraintNameInTemplate( IHttpRouteData routeData )
        {
            Contract.Requires( routeData != null );

            foreach ( var constraint in routeData.Route.Constraints )
            {
                if ( constraint.Value is ApiVersionRouteConstraint )
                {
                    return constraint.Key;
                }
            }

            return null;
        }
    }
}