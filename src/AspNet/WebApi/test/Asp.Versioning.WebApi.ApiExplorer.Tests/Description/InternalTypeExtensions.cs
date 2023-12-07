// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Description;

using System.Diagnostics;
using System.Reflection;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

internal static class InternalTypeExtensions
{
    internal static IHttpRoute NewRouteCollectionRoute()
    {
        var type = Type.GetType( "System.Web.Http.Routing.RouteCollectionRoute, System.Web.Http", throwOnError: true, ignoreCase: false );
        return (IHttpRoute) Activator.CreateInstance( type );
    }

    internal static void EnsureInitialized( this IHttpRoute route, Func<IReadOnlyCollection<IHttpRoute>> initializer )
    {
        Debug.Assert( route.GetType().Name == "RouteCollectionRoute", "Extension method only intended to support testing RouteCollectionRoute.EnsureInitialized" );

        var type = route.GetType();
        var method = type.GetRuntimeMethod( nameof( EnsureInitialized ), [initializer.GetType()] );

        method.Invoke( route, [initializer] );
    }

    internal static IDirectRouteBuilder NewDirectRouteBuilder( IReadOnlyCollection<HttpActionDescriptor> actions, bool targetIsAction )
    {
        var type = Type.GetType( "System.Web.Http.Routing.DirectRouteBuilder, System.Web.Http", throwOnError: true, ignoreCase: false );
        return (IDirectRouteBuilder) Activator.CreateInstance( type, actions, targetIsAction );
    }
}