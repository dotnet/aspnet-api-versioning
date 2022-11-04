// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;

internal sealed class EdgeBuilder
{
    private readonly bool versionsByUrl;
    private readonly bool unspecifiedNotAllowed;
    private readonly string constraintName;
    private readonly HashSet<EdgeKey> keys;
    private readonly Dictionary<EdgeKey, List<Endpoint>> edges;
    private EdgeKey assumeDefault = EdgeKey.AssumeDefault;
    private HashSet<RoutePattern>? routePatterns;

    public EdgeBuilder(
        int capacity,
        IApiVersionParameterSource source,
        ApiVersioningOptions options,
        ILogger logger )
    {
        versionsByUrl = source.VersionsByUrl();
        unspecifiedNotAllowed = !options.AssumeDefaultVersionWhenUnspecified;
        constraintName = options.RouteConstraintName;
        keys = new( capacity + 1 );
        edges = new( capacity + 6 )
        {
            [EdgeKey.Malformed] = new( capacity: 1 ) { new MalformedApiVersionEndpoint( logger ) },
            [EdgeKey.Ambiguous] = new( capacity: 1 ) { new AmbiguousApiVersionEndpoint( logger ) },
            [EdgeKey.Unspecified] = new( capacity: 1 ) { new UnspecifiedApiVersionEndpoint( logger ) },
            [EdgeKey.UnsupportedMediaType] = new( capacity: 1 ) { new UnsupportedMediaTypeEndpoint() },
            [EdgeKey.NotAcceptable] = new( capacity: 1 ) { new NotAcceptableEndpoint() },
        };
    }

    public IReadOnlyList<PolicyNodeEdge> Build() =>
        edges.Select( edge => new PolicyNodeEdge( edge.Key, edge.Value ) ).ToArray();

    public void Add( RouteEndpoint endpoint )
    {
        if ( unspecifiedNotAllowed )
        {
            return;
        }

        Add( ref assumeDefault, endpoint );
    }

    public void Add( RouteEndpoint endpoint, ApiVersion apiVersion )
    {
        var key = new EdgeKey( apiVersion );
        Add( ref key, endpoint );
    }

    private void Add( ref EdgeKey key, RouteEndpoint endpoint )
    {
        if ( keys.TryGetValue( key, out var existing ) )
        {
            key = existing;
        }
        else
        {
            keys.Add( key );
        }

        var routePattern = endpoint.RoutePattern;
        var needsRoutePattern = versionsByUrl && routePattern.HasVersionConstraint( constraintName );

        if ( needsRoutePattern )
        {
            routePatterns ??= new( new RoutePatternComparer() );
            needsRoutePattern &= routePatterns.Add( routePattern );

            if ( needsRoutePattern )
            {
                key.RoutePatterns.Add( routePattern );
            }
        }

        if ( !edges.TryGetValue( key, out var endpoints ) )
        {
            edges.Add( key, endpoints = new() );
        }

        endpoints.Add( endpoint );
    }
}