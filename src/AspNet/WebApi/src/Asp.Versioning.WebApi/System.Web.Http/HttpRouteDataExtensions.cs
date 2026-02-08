// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Web.Http;

using Asp.Versioning.Routing;
using System.Web.Http.Routing;

internal static class HttpRouteDataExtensions
{
    extension( IHttpRouteData routeData )
    {
        internal CandidateAction[]? DirectRouteCandidates
        {
            get
            {
                var subRoutes = routeData.GetSubRoutes();

                if ( subRoutes == null )
                {
                    if ( routeData.Route == null )
                    {
                        return null;
                    }

                    return routeData.Route.DirectRouteCandidates;
                }

                var list = new List<CandidateAction>();

                foreach ( var data in subRoutes )
                {
                    var directRouteCandidates = data.Route.DirectRouteCandidates;

                    if ( directRouteCandidates != null )
                    {
                        list.AddRange( directRouteCandidates );
                    }
                }

                return [.. list];
            }
        }
    }
}