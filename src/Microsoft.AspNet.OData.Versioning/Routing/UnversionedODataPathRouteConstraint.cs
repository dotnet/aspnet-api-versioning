namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static System.Web.Http.Routing.HttpRouteDirection;

    sealed class UnversionedODataPathRouteConstraint : IHttpRouteConstraint
    {
        readonly ApiVersion? apiVersion;
        readonly IEnumerable<IHttpRouteConstraint> innerConstraints;

        internal UnversionedODataPathRouteConstraint( IEnumerable<IHttpRouteConstraint> innerConstraints ) => this.innerConstraints = innerConstraints;

        internal UnversionedODataPathRouteConstraint( IHttpRouteConstraint innerConstraint, ApiVersion apiVersion )
        {
            innerConstraints = new[] { innerConstraint };
            this.apiVersion = apiVersion;
        }

        bool MatchAnyVersion => apiVersion == null;

        public bool Match( HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection )
        {
            if ( routeDirection == UriGeneration )
            {
                return true;
            }

            if ( !MatchAnyVersion && apiVersion != request.GetRequestedApiVersion() )
            {
                return false;
            }

            var properties = request.ApiVersionProperties();

            // determine whether this constraint can match any api version and no api version has otherwise been matched
            if ( MatchAnyVersion && properties.RequestedApiVersion == null )
            {
                var options = request.GetApiVersioningOptions();

                // is implicitly matching an api version allowed?
                if ( options.AssumeDefaultVersionWhenUnspecified || IsServiceDocumentOrMetadataRoute( values ) )
                {
                    var odata = request.ODataApiVersionProperties();
                    var model = new ApiVersionModel( odata.MatchingRoutes.Keys, Enumerable.Empty<ApiVersion>() );
                    var selector = options.ApiVersionSelector;
                    var requestedApiVersion = properties.RequestedApiVersion = selector.SelectVersion( request, model );

                    // if an api version is selected, determine if it corresponds to a route that has been previously matched
                    if ( requestedApiVersion != null && odata.MatchingRoutes.TryGetValue( requestedApiVersion, out var routeName ) )
                    {
                        // create a new versioned path constraint on the fly and evaluate it. this sets up the underlying odata
                        // infrastructure such as the container, edm, etc. this has no bearing the action selector which will
                        // already select the correct action. without this the response may be incorrect, even if the correct
                        // action is selected and executed.
                        var constraint = new VersionedODataPathRouteConstraint( routeName, requestedApiVersion );
                        return constraint.Match( request, route, parameterName, values, routeDirection );
                    }
                }
            }
            else if ( !MatchAnyVersion && properties.RequestedApiVersion != apiVersion )
            {
                return false;
            }

            request.DeleteRequestContainer( true );

            // by evaluating the remaining unversioned constraints, this will ultimately determine whether 400 or 404
            // is returned for an odata request
            foreach ( var constraint in innerConstraints )
            {
                if ( constraint.Match( request, route, parameterName, values, routeDirection ) )
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsServiceDocumentOrMetadataRoute( IDictionary<string, object> values ) =>
            values.TryGetValue( "odataPath", out var value ) && ( value == null || Equals( value, "$metadata" ) );
    }
}