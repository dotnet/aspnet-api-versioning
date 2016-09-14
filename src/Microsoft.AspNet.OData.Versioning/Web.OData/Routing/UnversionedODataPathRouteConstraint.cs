namespace Microsoft.Web.OData.Routing
{
    using Http;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static System.Web.Http.Routing.HttpRouteDirection;

    internal sealed class UnversionedODataPathRouteConstraint : IHttpRouteConstraint
    {
        private readonly ApiVersion apiVersion;
        private readonly IEnumerable<IHttpRouteConstraint> innerConstraints;

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

        private bool MatchAnyVersion => apiVersion == null;

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

            return innerConstraints.Any( c => c.Match( request, route, parameterName, values, routeDirection ) );
        }
    }
}