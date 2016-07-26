namespace System.Web.Http
{
    using Collections.Generic;
    using Controllers;
    using Diagnostics.Contracts;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Routing;
    using Routing;

    internal static class HttpRouteExtensions
    {
        internal static CandidateAction[] GetDirectRouteCandidates( this IHttpRoute route )
        {
            Contract.Requires( route != null );

            var dataTokens = route.DataTokens;

            if ( dataTokens == null )
            {
                return null;
            }

            var candidates = new List<CandidateAction>();
            var directRouteActions = default( HttpActionDescriptor[] );
            var possibleDirectRouteActions = default( HttpActionDescriptor[] );

            if ( dataTokens.TryGetValue( RouteDataTokenKeys.Actions, out possibleDirectRouteActions ) )
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

            var order = 0;
            var possibleOrder = 0;

            if ( dataTokens.TryGetValue( RouteDataTokenKeys.Order, out possibleOrder ) )
            {
                order = possibleOrder;
            }

            var precedence = 0m;
            var possiblePrecedence = 0m;

            if ( dataTokens.TryGetValue( RouteDataTokenKeys.Precedence, out possiblePrecedence ) )
            {
                precedence = possiblePrecedence;
            }

            foreach ( var actionDescriptor in directRouteActions )
            {
                candidates.Add( new CandidateAction( actionDescriptor, order, precedence ) );
            }

            return candidates.ToArray();
        }
    }
}
