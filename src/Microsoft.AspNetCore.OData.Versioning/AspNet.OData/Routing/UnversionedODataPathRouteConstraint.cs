namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using static Microsoft.AspNetCore.Routing.RouteDirection;

    sealed class UnversionedODataPathRouteConstraint : IRouteConstraint
    {
        readonly ApiVersion? apiVersion;
        readonly IEnumerable<IRouteConstraint> innerConstraints;

        internal UnversionedODataPathRouteConstraint( IEnumerable<IRouteConstraint> innerConstraints ) =>
            this.innerConstraints = innerConstraints;

        internal UnversionedODataPathRouteConstraint( IRouteConstraint innerConstraint, ApiVersion apiVersion )
        {
            innerConstraints = new[] { innerConstraint };
            this.apiVersion = apiVersion;
        }

        bool MatchAnyVersion => apiVersion == null;

        public bool Match( HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection )
        {
            if ( routeDirection == UrlGeneration )
            {
                return true;
            }

            var feature = httpContext.Features.Get<IApiVersioningFeature>();

            // determine whether this constraint can match any api version and no api version has otherwise been matched
            if ( MatchAnyVersion && feature.RequestedApiVersion == null )
            {
                var options = httpContext.RequestServices.GetRequiredService<IOptions<ApiVersioningOptions>>().Value;

                // is implicitly matching an api version allowed?
                if ( options.AssumeDefaultVersionWhenUnspecified || IsServiceDocumentOrMetadataRoute( values ) )
                {
                    var odata = httpContext.ODataVersioningFeature();
                    var model = new ApiVersionModel( odata.MatchingRoutes.Keys, Array.Empty<ApiVersion>() );
                    var selector = httpContext.RequestServices.GetRequiredService<IApiVersionSelector>();
                    var requestedApiVersion = feature.RequestedApiVersion = selector.SelectVersion( httpContext.Request, model );

                    // if an api version is selected, determine if it corresponds to a route that has been previously matched
                    if ( requestedApiVersion != null && odata.MatchingRoutes.TryGetValue( requestedApiVersion, out var routeName ) )
                    {
                        // create a new versioned path constraint on the fly and evaluate it. this sets up the underlying odata
                        // infrastructure such as the container, edm, etc. this has no bearing the action selector which will
                        // already select the correct action. without this the response may be incorrect, even if the correct
                        // action is selected and executed.
                        var constraint = new VersionedODataPathRouteConstraint( routeName, requestedApiVersion );
                        return constraint.Match( httpContext, route, routeKey, values, routeDirection );
                    }
                }
            }
            else if ( !MatchAnyVersion && feature.RequestedApiVersion != apiVersion )
            {
                return false;
            }

            httpContext.Request.DeleteRequestContainer( true );

            // by evaluating the remaining unversioned constraints, this will ultimately determine whether 400 or 404
            // is returned for an odata request
            foreach ( var constraint in innerConstraints )
            {
                if ( constraint.Match( httpContext, route, routeKey, values, routeDirection ) )
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsServiceDocumentOrMetadataRoute( RouteValueDictionary values ) =>
            values.TryGetValue( "odataPath", out var value ) && ( value == null || Equals( value, "$metadata" ) );
    }
}