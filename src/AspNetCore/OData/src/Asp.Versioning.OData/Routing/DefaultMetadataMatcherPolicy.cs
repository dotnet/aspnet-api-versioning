// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
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
        var constraintName = options.Value.RouteConstraintName;

        for ( var i = 0; i < endpoints.Count; i++ )
        {
            var endpoint = endpoints[i];

            if ( !IsServiceDocumentOrMetadataEndpoint( endpoint.Metadata ) )
            {
                continue;
            }

            edges ??= [];
            edges.Add( endpoint );

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

            if ( endpoint is not RouteEndpoint route )
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
            return Array.Empty<PolicyNodeEdge>();
        }

        var state = (lowestApiVersion, routePatterns?.ToArray() ?? []);
        return new PolicyNodeEdge[] { new( state, edges ) };
    }

    /// <inheritdoc />
    public PolicyJumpTable BuildJumpTable( int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges )
    {
        ArgumentNullException.ThrowIfNull( edges );

        Debug.Assert( edges.Count == 1, $"Only a single edge was expected, but {edges.Count} edges were provided" );

        var edge = edges[0];
        var (implicitApiVersion, routePatterns) = ((ApiVersion, RoutePattern[])) edge.State;

        return new MetadataJumpTable(
            edge.Destination,
            implicitApiVersion,
            routePatterns,
            options.Value.RouteConstraintName,
            versionsByUrl );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int ApiVersioningPolicy() =>
        new ApiVersionMatcherPolicy(
            ApiVersionParser.Default,
            Enumerable.Empty<IApiVersionMetadataCollationProvider>(),
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
        private readonly string constraintName;
        private readonly bool versionsByUrl;

        internal MetadataJumpTable(
            int implicitDestination,
            ApiVersion implicitApiVersion,
            IReadOnlyList<RoutePattern> routePatterns,
            string constraintName,
            bool versionsByUrl )
        {
            this.implicitDestination = implicitDestination;
            this.implicitApiVersion = implicitApiVersion;
            this.routePatterns = routePatterns;
            this.constraintName = constraintName;
            this.versionsByUrl = versionsByUrl;
        }

        public override int GetDestination( HttpContext httpContext )
        {
            // ~/$metadata is special. the backing controller is not version-neutral.
            // to maintain backward compatibility, if no api version is explicitly
            // specified, then default to the lowest defined version.
            //
            // we don't want to set an implicit api version if it exists in the path
            // because the normal routing process will handle it. it isn't available
            // from the feature because route constraints haven't been evaluated yet
            var feature = httpContext.ApiVersioningFeature();
            var needsImplicitApiVersion =
                feature.RawRequestedApiVersions.Count == 0 &&
                ( !versionsByUrl ||
                  !httpContext.Request.TryGetApiVersionFromPath( routePatterns, constraintName, out _ ) );

            if ( needsImplicitApiVersion )
            {
                feature.RequestedApiVersion = implicitApiVersion;
            }

            return implicitDestination;
        }
    }
}