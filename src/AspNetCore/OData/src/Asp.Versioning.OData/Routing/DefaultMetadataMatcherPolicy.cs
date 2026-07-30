// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using static Asp.Versioning.ApiVersionMapping;

/// <summary>
/// Represents the <see cref="MatcherPolicy">matcher policy</see> for the default OData
/// service document and $metadata endpoint.
/// </summary>
[CLSCompliant( false )]
public class DefaultMetadataMatcherPolicy : MatcherPolicy, INodeBuilderPolicy
{
    private static int BeforeApiVersioningPolicy { get; } = ApiVersioningPolicy() - 100;
    private readonly bool versionsByUrl;
    private readonly IOptions<ApiVersioningOptions> options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultMetadataMatcherPolicy"/> class.
    /// </summary>
    /// <param name="parameterSource">The <see cref="IApiVersionParameterSource">API version parameter source</see>.</param>
    /// <param name="options">The configured <see cref="ApiVersioningOptions">API versioning options</see>.</param>
    public DefaultMetadataMatcherPolicy(
        IApiVersionParameterSource parameterSource,
        IOptions<ApiVersioningOptions> options )
    {
        ArgumentNullException.ThrowIfNull( parameterSource );
        versionsByUrl = parameterSource.VersionsByUrl();
        this.options = options;
    }

    /// <inheritdoc />
    public override int Order { get; } = BeforeApiVersioningPolicy;

    /// <inheritdoc />
    public virtual bool AppliesToEndpoints( IReadOnlyList<Endpoint> endpoints )
    {
        ArgumentNullException.ThrowIfNull( endpoints );

        for ( var i = 0; i < endpoints.Count; i++ )
        {
            if ( IsServiceDocumentOrMetadataEndpoint( endpoints[i].Metadata ) )
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public IReadOnlyList<PolicyNodeEdge> GetEdges( IReadOnlyList<Endpoint> endpoints )
    {
        ArgumentNullException.ThrowIfNull( endpoints );

        var edges = default( List<Endpoint> );
        var lowestApiVersion = default( ApiVersion );
        var routePatterns = default( HashSet<RoutePattern> );
        var metadataPatterns = default( HashSet<RoutePattern> );
        var constraintName = options.Value.RouteConstraintName;
        var hasOtherEndpoints = false;
        var canTestRequestPath = true;

        for ( var i = 0; i < endpoints.Count; i++ )
        {
            var endpoint = endpoints[i];

            // every endpoint must remain in an edge. an endpoint that is excluded from all edges is dropped from the
            // matcher and becomes unreachable. a node is normally expected to only contain the service document and/or
            // $metadata, but it can also contain unrelated endpoints when another route template has a non-literal
            // segment in the same position. for example, ~/Orders({key}) and ~/v{version:apiVersion} are both complex
            // segments so they share the same node
            edges ??= [];
            edges.Add( endpoint );

            if ( !IsServiceDocumentOrMetadataEndpoint( endpoint.Metadata ) )
            {
                hasOtherEndpoints = true;
                continue;
            }

            var route = endpoint as RouteEndpoint;

            if ( route is null )
            {
                canTestRequestPath = false;
            }
            else
            {
                metadataPatterns ??= new( new RoutePatternComparer() );
                metadataPatterns.Add( route.RoutePattern );
            }

            var model = endpoint.Metadata.GetMetadata<ApiVersionMetadata>()!.Map( Explicit | Implicit );
            var versions = model.DeclaredApiVersions;

            if ( versions.Count == 0 )
            {
                continue;
            }

            var current = versions[0];

            if ( lowestApiVersion == null )
            {
                lowestApiVersion = current;
            }
            else if ( current.CompareTo( lowestApiVersion ) < 0 )
            {
                lowestApiVersion = current;
            }

            if ( route is null )
            {
                continue;
            }

            var routePattern = route.RoutePattern;
            var needsRoutePattern = versionsByUrl && routePattern.HasVersionConstraint( constraintName );

            if ( needsRoutePattern )
            {
                routePatterns ??= new( new RoutePatternComparer() );
                routePatterns.Add( routePattern );
            }
        }

        if ( edges is null || lowestApiVersion is null )
        {
            return [];
        }

        // the request path only has to be tested when the node contains unrelated endpoints. when it does not, any
        // request that reaches the node is for the service document or $metadata and no additional matching is required
        var pathPatterns = hasOtherEndpoints && canTestRequestPath
                           ? metadataPatterns?.ToArray() ?? []
                           : [];

        var state = (lowestApiVersion, routePatterns?.ToArray() ?? [], pathPatterns);
        return [new( state, edges )];
    }

    /// <inheritdoc />
    public PolicyJumpTable BuildJumpTable( int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges )
    {
        ArgumentNullException.ThrowIfNull( edges );

        Debug.Assert( edges.Count == 1, $"Only a single edge was expected, but {edges.Count} edges were provided" );

        var edge = edges[0];
        var (implicitApiVersion, routePatterns, metadataPatterns) = ((ApiVersion, RoutePattern[], RoutePattern[])) edge.State;

        return new MetadataJumpTable(
            edge.Destination,
            implicitApiVersion,
            routePatterns,
            metadataPatterns,
            options.Value.RouteConstraintName,
            versionsByUrl );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int ApiVersioningPolicy() =>
        new ApiVersionMatcherPolicy(
            ApiVersionParser.Default,
            [],
            Options.Create( new ApiVersioningOptions() ),
            new NullLogger<ApiVersionMatcherPolicy>() ).Order;

    private static bool IsServiceDocumentOrMetadataEndpoint( EndpointMetadataCollection metadata )
    {
        var odata = metadata.GetMetadata<IODataRoutingMetadata>();

        if ( odata == null )
        {
            return false;
        }

        var template = odata.Template;

        return template.Count == 0 || ( template.Count == 1 && template[0] is MetadataSegmentTemplate );
    }

    private sealed class MetadataJumpTable : PolicyJumpTable
    {
        private readonly int implicitDestination;
        private readonly ApiVersion implicitApiVersion;
        private readonly IReadOnlyList<RoutePattern> routePatterns;
        private readonly IReadOnlyList<RoutePattern> metadataPatterns;
        private readonly string constraintName;
        private readonly bool versionsByUrl;

        internal MetadataJumpTable(
            int implicitDestination,
            ApiVersion implicitApiVersion,
            IReadOnlyList<RoutePattern> routePatterns,
            IReadOnlyList<RoutePattern> metadataPatterns,
            string constraintName,
            bool versionsByUrl )
        {
            this.implicitDestination = implicitDestination;
            this.implicitApiVersion = implicitApiVersion;
            this.routePatterns = routePatterns;
            this.metadataPatterns = metadataPatterns;
            this.constraintName = constraintName;
            this.versionsByUrl = versionsByUrl;
        }

        public override int GetDestination( HttpContext httpContext )
        {
            // ~/$metadata is special. the backing controller is not version-neutral. to maintain backward compatibility,
            // if no api version is explicitly specified, then default to the lowest defined version. we don't want to
            // set an implicit api version if it exists in the path because the normal routing process will handle it.
            // it isn't available from the feature because route constraints haven't been evaluated yet
            var feature = httpContext.ApiVersioningFeature;
            var needsImplicitApiVersion =
                feature.RawRequestedApiVersions.Count == 0 &&
                ( !versionsByUrl
                  || !httpContext.Request.TryGetApiVersionFromPath( routePatterns, constraintName, out _ ) )
                  && IsServiceDocumentOrMetadata( httpContext.Request );

            if ( needsImplicitApiVersion )
            {
                feature.RequestedApiVersion = implicitApiVersion;
            }

            return implicitDestination;
        }

        // an implicit api version must only be applied to the service document or $metadata. when the node contains
        // unrelated endpoints, applying it to all of them would make a normal endpoint silently resolve to the lowest
        // api version instead of reporting an unspecified api version
        private bool IsServiceDocumentOrMetadata( HttpRequest request )
        {
            if ( metadataPatterns.Count == 0 )
            {
                return true;
            }

            var path = request.Path;
            var values = default( RouteValueDictionary );

            for ( var i = 0; i < metadataPatterns.Count; i++ )
            {
                var routePattern = metadataPatterns[i];
                var defaults = new RouteValueDictionary( routePattern.RequiredValues );
                var matcher = new TemplateMatcher( new( routePattern ), defaults );

                if ( values is null )
                {
                    values = [];
                }
                else
                {
                    values.Clear();
                }

                if ( matcher.TryMatch( path, values ) )
                {
                    return true;
                }
            }

            return false;
        }
    }
}