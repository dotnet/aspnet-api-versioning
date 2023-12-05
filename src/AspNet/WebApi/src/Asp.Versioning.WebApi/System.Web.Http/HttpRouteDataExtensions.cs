// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using Asp.Versioning.Routing;
using System.Web.Http.Routing;

internal static class HttpRouteDataExtensions
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

        return [.. list];
    }
}