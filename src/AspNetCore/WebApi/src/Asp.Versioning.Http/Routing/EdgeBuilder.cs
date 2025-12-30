// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;

internal sealed class EdgeBuilder
{
    private const int RejectionEndpointCapacity = NumberOfRejectionEndpoints + 1;
    internal const int NumberOfRejectionEndpoints = 6;
    private readonly bool versionsByUrl;
    private readonly bool unspecifiedAllowed;
    private readonly string constraintName;
    private readonly HashSet<EdgeKey> keys;
    private readonly Dictionary<EdgeKey, List<Endpoint>> edges;
    private readonly HashSet<RoutePattern> routePatterns = new( new RoutePatternComparer() );
    private EdgeKey assumeDefault = EdgeKey.AssumeDefault;

    public EdgeBuilder(
        int capacity,
        IApiVersionParameterSource source,
        ApiVersioningOptions options,
        ILogger logger )
    {
        versionsByUrl = source.VersionsByUrl();
        unspecifiedAllowed = options.AssumeDefaultVersionWhenUnspecified;
        constraintName = options.RouteConstraintName;
        keys = new( capacity + 1 );
        edges = new( capacity + RejectionEndpointCapacity )
        {
            [EdgeKey.Malformed] = [new MalformedApiVersionEndpoint( logger, options )],
            [EdgeKey.Ambiguous] = [new AmbiguousApiVersionEndpoint( logger )],
            [EdgeKey.Unspecified] = [new UnspecifiedApiVersionEndpoint( logger, options )],
            [EdgeKey.Unsupported] = [new UnsupportedApiVersionEndpoint( options )],
            [EdgeKey.UnsupportedMediaType] = [new UnsupportedMediaTypeEndpoint( options )],
            [EdgeKey.NotAcceptable] = [new NotAcceptableEndpoint( options )],
        };
    }

    public IReadOnlyList<PolicyNodeEdge> Build()
    {
        routePatterns.TrimExcess();
        return [.. edges.Select( edge => new PolicyNodeEdge( edge.Key, edge.Value ) )];
    }

    public void Add( RouteEndpoint endpoint )
    {
        if ( unspecifiedAllowed )
        {
            Add( ref assumeDefault, endpoint );
        }
    }

    public void Add( RouteEndpoint endpoint, ApiVersion apiVersion, ApiVersionMetadata metadata )
    {
        // use a singleton of all route patterns that version by url segment. this is needed to extract the value for
        // selecting a destination in the jump table. any matching template will do and every edge should have the same
        // list known through the application, which may be zero
        var key = new EdgeKey( apiVersion, metadata, routePatterns );

        Add( ref key, endpoint );

        // include version-neutral endpoints when assuming the default so they are also considered when unspecified
        if ( unspecifiedAllowed && metadata.IsApiVersionNeutral && apiVersion == ApiVersion.Neutral )
        {
            Add( ref assumeDefault, endpoint );
        }
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
            routePatterns.Add( routePattern );
        }

        if ( !edges.TryGetValue( key, out var endpoints ) )
        {
            edges.Add( key, endpoints = [] );
        }

        endpoints.Add( endpoint );
    }
}