// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace System.Web.Http;

using System.Web.Http.Routing;

internal static class HttpRouteCollectionExtensions
{
    internal static string? GetRouteName( this HttpRouteCollection routes, IHttpRoute route )
    {
        foreach ( var item in routes.ToDictionary() )
        {
            if ( Equals( item.Value, route ) )
            {
                return item.Key;
            }
        }

        return default;
    }
}