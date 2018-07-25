﻿namespace Microsoft.AspNet.OData.Routing
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.Web.Http;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static System.Web.Http.Routing.HttpRouteDirection;

    sealed class UnversionedODataPathRouteConstraint : IHttpRouteConstraint
    {
        readonly ApiVersion apiVersion;
        readonly IEnumerable<IHttpRouteConstraint> innerConstraints;

        internal UnversionedODataPathRouteConstraint( IEnumerable<IHttpRouteConstraint> innerConstraints )
        {
            Contract.Requires( innerConstraints != null );
            this.innerConstraints = innerConstraints;
        }

        internal UnversionedODataPathRouteConstraint( IHttpRouteConstraint innerConstraint, ApiVersion apiVersion )
        {
            Contract.Requires( innerConstraint != null );

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

            request.DeleteRequestContainer( true );

            foreach ( var constraint in innerConstraints )
            {
                if ( constraint.Match( request, route, parameterName, values, routeDirection ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}