namespace System.Web.Http
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Routing;
    using System.Collections.Generic;
    using System.Web.Http.Controllers;
    using System.Web.Http.Routing;

    static class HttpRouteExtensions
    {
        internal static CandidateAction[]? GetDirectRouteCandidates( this IHttpRoute route )
        {
            var dataTokens = route.DataTokens;

            if ( dataTokens == null )
            {
                return null;
            }

            var candidates = new List<CandidateAction>();
            var directRouteActions = default( HttpActionDescriptor[] );

            if ( dataTokens.TryGetValue( RouteDataTokenKeys.Actions, out HttpActionDescriptor[] possibleDirectRouteActions ) )
            {
                if ( possibleDirectRouteActions != null && possibleDirectRouteActions.Length > 0 )
                {
                    directRouteActions = possibleDirectRouteActions;
                }
            }

            if ( directRouteActions == null )
            {
                return null;
            }

            if ( !dataTokens.TryGetValue( RouteDataTokenKeys.Order, out int order ) )
            {
                order = 0;
            }

            if ( !dataTokens.TryGetValue( RouteDataTokenKeys.Precedence, out decimal precedence ) )
            {
                precedence = 0m;
            }

            foreach ( var actionDescriptor in directRouteActions )
            {
                candidates.Add( new CandidateAction( actionDescriptor, order, precedence ) );
            }

            return candidates.ToArray();
        }
    }
}