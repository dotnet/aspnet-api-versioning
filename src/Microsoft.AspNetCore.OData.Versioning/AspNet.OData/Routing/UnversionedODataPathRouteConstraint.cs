namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using static Microsoft.AspNetCore.Routing.RouteDirection;

    sealed class UnversionedODataPathRouteConstraint : IRouteConstraint
    {
        readonly ApiVersion apiVersion;
        readonly IEnumerable<IRouteConstraint> innerConstraints;

        internal UnversionedODataPathRouteConstraint( IEnumerable<IRouteConstraint> innerConstraints )
        {
            Contract.Requires( innerConstraints != null );
            this.innerConstraints = innerConstraints;
        }

        internal UnversionedODataPathRouteConstraint( IRouteConstraint innerConstraint, ApiVersion apiVersion )
        {
            Contract.Requires( innerConstraint != null );

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

            if ( !MatchAnyVersion )
            {
                var feature = httpContext.Features.Get<IApiVersioningFeature>();

                if ( feature.RequestedApiVersion != apiVersion )
                {
                    return false;
                }
            }

            httpContext.Request.DeleteRequestContainer( true );

            foreach ( var constraint in innerConstraints )
            {
                if ( constraint.Match( httpContext, route, routeKey, values, routeDirection ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}