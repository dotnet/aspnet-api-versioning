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
    private List<Endpoint>? unversioned;

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
            [EdgeKey.Malformed] = [MalformedApiVersionEndpoint.New( logger, options )],
            [EdgeKey.Ambiguous] = [AmbiguousApiVersionEndpoint.New( logger )],
            [EdgeKey.Unspecified] = [UnspecifiedApiVersionEndpoint.New( logger, options )],
            [EdgeKey.Unsupported] = [UnsupportedApiVersionEndpoint.New( options )],
            [EdgeKey.UnsupportedMediaType] = [UnsupportedMediaTypeEndpoint.New( options )],
            [EdgeKey.NotAcceptable] = [NotAcceptableEndpoint.New( options )],
        };
    }

    public IReadOnlyList<PolicyNodeEdge> Build()
    {
        routePatterns.TrimExcess();

        if ( unversioned is not null )
        {
            // an endpoint without ApiVersionMetadata is not versioned and must never have an API versioning policy
            // enforced against it. the endpoints of an edge become the candidates of the destination it jumps to,
            // which means an endpoint that is omitted from an edge is unreachable. carry the unversioned endpoints
            // into every edge so they remain a candidate for any destination the jump table can select. a client error
            // endpoint always sorts last, so an unversioned endpoint that matches will be selected ahead of it
            foreach ( var endpoints in edges.Values )
            {
                endpoints.AddRange( unversioned );
            }

            // the jump table can also exit to the destination of the enclosing node; for example, a 404 when versioning
            // by url segment only. that destination has no edge, so provide one that the policy can redirect the exit to
            edges.Add( EdgeKey.Unversioned, [.. unversioned] );
        }

        return [.. edges.Select( edge => new PolicyNodeEdge( edge.Key, edge.Value ) )];
    }

    public void AddUnversioned( Endpoint endpoint ) => ( unversioned ??= [] ).Add( endpoint );

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