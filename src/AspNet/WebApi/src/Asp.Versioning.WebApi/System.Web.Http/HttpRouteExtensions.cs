// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning.Routing;
using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

internal static class HttpRouteExtensions
{
    internal static CandidateAction[]? GetDirectRouteCandidates( this IHttpRoute route )
    {
        var dataTokens = route.DataTokens;

        if ( dataTokens == null )
        {
            return null;
        }

        var directRouteActions = default( HttpActionDescriptor[] );

        if ( dataTokens.TryGetValue( RouteDataTokenKeys.Actions, out HttpActionDescriptor[]? possibleDirectRouteActions ) &&
            possibleDirectRouteActions != null &&
            possibleDirectRouteActions.Length > 0 )
        {
            directRouteActions = possibleDirectRouteActions;
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

        var candidates = new List<CandidateAction>( capacity: directRouteActions.Length );

        for ( var i = 0; i < directRouteActions.Length; i++ )
        {
            candidates.Add( new CandidateAction( directRouteActions[i], order, precedence ) );
        }

        return [.. candidates];
    }
}