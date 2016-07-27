namespace System.Web.Http
{
    using Collections.Generic;
    using Diagnostics.Contracts;
    using Microsoft.Web.Http.Routing;
    using Routing;

    internal static class HttpRouteDataExtensions
    {
        internal static CandidateAction[] GetDirectRouteCandidates( this IHttpRouteData routeData )
        {
            Contract.Requires( routeData != null );

            var subRoutes = routeData.GetSubRoutes();

            if ( subRoutes == null )
            {
                if ( routeData.Route == null )
                {
                    return null;
                }

                return routeData.Route.GetDirectRouteCandidates();
            }

            var list = new List<CandidateAction>();

            foreach ( var data in subRoutes )
            {
                var directRouteCandidates = data.Route.GetDirectRouteCandidates();

                if ( directRouteCandidates != null )
                {
                    list.AddRange( directRouteCandidates );
                }
            }

            return list.ToArray();
        }
    }
}
