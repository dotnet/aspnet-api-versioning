// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Web.Http;

using System.Web.Http.Routing;

internal static class HttpRouteCollectionExtensions
{
    extension( HttpRouteCollection routes )
    {
        internal string? GetRouteName( IHttpRoute route )
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
}