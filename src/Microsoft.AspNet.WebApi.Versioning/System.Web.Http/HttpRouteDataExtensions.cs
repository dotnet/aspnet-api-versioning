namespace System.Web.Http
{
    using Microsoft.Web.Http.Routing;
    using System.Collections.Generic;
    using System.Web.Http.Routing;

    static class HttpRouteDataExtensions
    {
        internal static CandidateAction[]? GetDirectRouteCandidates( this IHttpRouteData routeData )
        {
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