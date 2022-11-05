// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System.Reflection;
using static System.Linq.Expressions.Expression;

/// <summary>
/// Defines the behavior of a route parser.
/// </summary>
/// <remarks>This class serves as an adapter over the built-in ASP.NET Web API RouteParser class and
/// enables the ability to override the behavior of parsing a route.</remarks>
public class RouteParser
{
    private static readonly Lazy<Func<IParsedRoute>> createNewFunc = new( CreateNewFunc );
    private static readonly Lazy<Func<string, object>> parseFunc = new( NewParseFunc );

    /// <summary>
    /// Creates a new, parsed route.
    /// </summary>
    /// <returns>A new, <see cref="IParsedRoute">parsed route</see>.</returns>
    public virtual IParsedRoute CreateNew() => createNewFunc.Value();

    /// <summary>
    /// Parses the specified route template.
    /// </summary>
    /// <param name="routeTemplate">The route template to parse.</param>
    /// <returns>A <see cref="IParsedRoute">parsed route</see>.</returns>
    public virtual IParsedRoute Parse( string routeTemplate )
    {
        var parsedRoute = parseFunc.Value( routeTemplate );
        var adapterType = typeof( ParsedRouteAdapter<> ).MakeGenericType( parsedRoute.GetType() );
        var adapter = (IParsedRoute) Activator.CreateInstance( adapterType, parsedRoute );

        return adapter;
    }

    private static Func<IParsedRoute> CreateNewFunc()
    {
        var pathSegmentType = Type.GetType( "System.Web.Http.Routing.PathSegment, System.Web.Http", throwOnError: true, ignoreCase: false );
        var parsedRouteType = Type.GetType( "System.Web.Http.Routing.HttpParsedRoute, System.Web.Http", throwOnError: true, ignoreCase: false );
        var adapterType = typeof( ParsedRouteAdapter<> ).MakeGenericType( parsedRouteType );
        var listType = typeof( List<> ).MakeGenericType( pathSegmentType );
        var ctor = parsedRouteType.GetConstructors().Single(
            c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType.Equals( listType );
            } );
        var newList = New( listType );
        var newParsedRoute = New( ctor, newList );
        var newAdapter = New( adapterType.GetConstructors().Single(), newParsedRoute );
        var lambda = Lambda<Func<IParsedRoute>>( newAdapter );

        return lambda.Compile();
    }

    private static Func<string, object> NewParseFunc()
    {
        var routeParserType = Type.GetType( "System.Web.Http.Routing.RouteParser, System.Web.Http", throwOnError: true, ignoreCase: false );
        var routeTemplate = Parameter( typeof( string ), "routeTemplate" );
        var parse = routeParserType.GetRuntimeMethod( nameof( Parse ), new[] { typeof( string ) } );
        var body = Call( parse, routeTemplate );
        var lambda = Lambda<Func<string, object>>( body, routeTemplate );

        return lambda.Compile();
    }
}