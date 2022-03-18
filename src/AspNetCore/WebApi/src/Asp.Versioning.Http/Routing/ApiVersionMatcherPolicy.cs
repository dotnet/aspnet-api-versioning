// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static Asp.Versioning.ApiVersionMapping;

/// <summary>
/// Represents the <see cref="MatcherPolicy">matcher policy</see> for API versions.
/// </summary>
[CLSCompliant( false )]
public sealed class ApiVersionMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy, INodeBuilderPolicy
{
    private readonly IOptions<ApiVersioningOptions> options;
    private readonly IApiVersionParser apiVersionParser;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionMatcherPolicy"/> class.
    /// </summary>
    /// <param name="apiVersionParser">The <see cref="IApiVersionParser">parser</see> used to parse API versions.</param>
    /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the matcher policy.</param>
    /// <param name="logger">The <see cref="ILogger{T}">logger</see> used by the matcher policy.</param>
    public ApiVersionMatcherPolicy(
        IApiVersionParser apiVersionParser,
        IOptions<ApiVersioningOptions> options,
        ILogger<ApiVersionMatcherPolicy> logger )
    {
        this.apiVersionParser = apiVersionParser ?? throw new ArgumentNullException( nameof( apiVersionParser ) );
        this.options = options ?? throw new ArgumentNullException( nameof( options ) );
        this.logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
    }

    /// <inheritdoc />
    public override int Order { get; } = BeforeDefaultMatcherPolicy();

    private ApiVersioningOptions Options => options.Value;

    private IApiVersionParameterSource ApiVersionSource => options.Value.ApiVersionReader;

    private IApiVersionSelector ApiVersionSelector => options.Value.ApiVersionSelector;

    /// <inheritdoc />
    public bool AppliesToEndpoints( IReadOnlyList<Endpoint> endpoints )
    {
        if ( endpoints == null )
        {
            throw new ArgumentNullException( nameof( endpoints ) );
        }

        for ( var i = 0; i < endpoints.Count; i++ )
        {
            if ( endpoints[i].Metadata.GetMetadata<ApiVersionMetadata>() != null )
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public Task ApplyAsync( HttpContext httpContext, CandidateSet candidates )
    {
        if ( httpContext == null )
        {
            throw new ArgumentNullException( nameof( httpContext ) );
        }

        if ( candidates == null )
        {
            throw new ArgumentNullException( nameof( candidates ) );
        }

        var feature = httpContext.ApiVersioningFeature();
        var apiVersion = feature.RequestedApiVersion;

        if ( apiVersion == null && Options.AssumeDefaultVersionWhenUnspecified )
        {
            apiVersion = TrySelectApiVersion( httpContext, candidates );
            feature.RequestedApiVersion = apiVersion;
        }

        var (matched, hasCandidates) = MatchApiVersion( candidates, apiVersion );

        if ( !matched && hasCandidates && !DifferByRouteConstraintsOnly( candidates ) )
        {
            var builder = new ClientErrorEndpointBuilder( feature, candidates, logger );
            httpContext.SetEndpoint( builder.Build() );
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public PolicyJumpTable BuildJumpTable( int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges )
    {
        if ( edges == null )
        {
            throw new ArgumentNullException( nameof( edges ) );
        }

        const int NumberOfRejectionEndpoints = 4;
        var rejection = new RouteDestination( exitDestination );
        var capacity = edges.Count - NumberOfRejectionEndpoints;
        var destinations = new Dictionary<ApiVersion, int>( capacity );
        var source = ApiVersionSource;
        var versionsByUrl = source.VersionsByUrl();
        var routePatterns = default( List<RoutePattern> );

        for ( var i = 0; i < edges.Count; i++ )
        {
            var edge = edges[i];
            var state = (EdgeKey) edge.State;
            var version = state.ApiVersion;

            switch ( state.EndpointType )
            {
                case EndpointType.Ambiguous:
                    rejection.Ambiguous = edge.Destination;
                    break;
                case EndpointType.Malformed:
                    rejection.Malformed = edge.Destination;
                    break;
                case EndpointType.Unspecified:
                    rejection.Unspecified = edge.Destination;
                    break;
                case EndpointType.UnsupportedMediaType:
                    rejection.UnsupportedMediaType = edge.Destination;
                    break;
                case EndpointType.AssumeDefault:
                    rejection.AssumeDefault = edge.Destination;
                    break;
                default:
                    if ( versionsByUrl && state.RoutePatterns.Count > 0 )
                    {
                        routePatterns ??= new();
                        routePatterns.AddRange( state.RoutePatterns );
                    }

                    destinations.Add( version, edge.Destination );
                    break;
            }
        }

        return new ApiVersionPolicyJumpTable(
            rejection,
            destinations,
            routePatterns ?? (IReadOnlyList<RoutePattern>) Array.Empty<RoutePattern>(),
            apiVersionParser,
            source,
            Options );
    }

    /// <inheritdoc />
    public IReadOnlyList<PolicyNodeEdge> GetEdges( IReadOnlyList<Endpoint> endpoints )
    {
        if ( endpoints == null )
        {
            throw new ArgumentNullException( nameof( endpoints ) );
        }

        var capacity = endpoints.Count;
        var builder = new EdgeBuilder( capacity, ApiVersionSource, Options, logger );
        var versions = new SortedSet<ApiVersion>();
        var neutralEndpoints = default( List<RouteEndpoint> );
        var versionedEndpoints = new (RouteEndpoint, ApiVersionModel)[capacity];
        var count = 0;

        for ( var i = 0; i < endpoints.Count; i++ )
        {
            if ( endpoints[i] is not RouteEndpoint endpoint ||
                 endpoint.Metadata.GetMetadata<ApiVersionMetadata>() is not ApiVersionMetadata metadata )
            {
                continue;
            }

            var model = metadata.Map( Explicit | Implicit );

            if ( model.IsApiVersionNeutral )
            {
                builder.Add( endpoint, ApiVersion.Neutral );
                neutralEndpoints ??= new();
                neutralEndpoints.Add( endpoint );
            }
            else
            {
                builder.Add( endpoint );
                versionedEndpoints[count++] = (endpoint, model);
                versions.AddRange( model.DeclaredApiVersions );
            }
        }

        foreach ( var version in versions )
        {
            for ( var j = 0; j < count; j++ )
            {
                var (endpoint, model) = versionedEndpoints[j];
                var mappedWithImplementation = model.ImplementedApiVersions.Contains( version );

                if ( mappedWithImplementation )
                {
                    builder.Add( endpoint, version );
                }
            }

            if ( neutralEndpoints is null )
            {
                continue;
            }

            // add an edge for all known versions because version-neutral endpoints can map to any api version
            for ( var j = 0; j < neutralEndpoints.Count; j++ )
            {
                builder.Add( neutralEndpoints[j], version );
            }
        }

        return builder.Build();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int BeforeDefaultMatcherPolicy() => new HttpMethodMatcherPolicy().Order - 1000;

    private static bool DifferByRouteConstraintsOnly( CandidateSet candidates )
    {
        if ( candidates.Count < 2 )
        {
            return false;
        }

        // HACK: edge case where the only differences are route template semantics.
        // the established behavior is 400 when an endpoint 'could' match, but doesn't.
        // this will not work for the scenario:
        //
        // * 1.0 = values/{id}
        // * 2.0 = values/{id:int}
        //
        // Where the requested version is 2.0 and {id} is 'abc'. Users expect 404 in this
        // scenario. Both candidates have been eliminated, but the policy doesn't know why.
        // the only differences are route constraints; otherwise, the templates are equivalent.
        //
        // for the scenario:
        //
        // * 1.0 = values/{id}
        // * 2.0 = values/{id}
        //
        // but 3.0 is requested, 400 should be returned if we made it this far
        const string ReplacementPattern = "{$1}";
        var pattern = new Regex( "{([^:]+):[^}]+}", RegexOptions.Singleline | RegexOptions.IgnoreCase );
        var comparer = StringComparer.OrdinalIgnoreCase;
        string? template = default;
        string? normalizedTemplate = default;

        for ( var i = 0; i < candidates.Count; i++ )
        {
            ref var candidate = ref candidates[i];

            if ( candidate.Endpoint is not RouteEndpoint endpoint )
            {
                return false;
            }

            var otherTemplate = endpoint.RoutePattern.RawText ?? string.Empty;

            if ( template is null )
            {
                template = otherTemplate;
                normalizedTemplate = pattern.Replace( otherTemplate, ReplacementPattern );
            }
            else if ( !comparer.Equals( template, otherTemplate ) )
            {
                var normalizedOtherTemplate = pattern.Replace( otherTemplate, ReplacementPattern );

                if ( comparer.Equals( normalizedTemplate, normalizedOtherTemplate ) )
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static (bool Matched, bool HasCandidates) MatchApiVersion( CandidateSet candidates, ApiVersion? apiVersion )
    {
        List<int>? bestMatches = default;
        List<int>? implicitMatches = default;
        var hasCandidates = false;

        for ( var i = 0; i < candidates.Count; i++ )
        {
            if ( !candidates.IsValidCandidate( i ) )
            {
                continue;
            }

            hasCandidates = true;
            ref var candidate = ref candidates[i];
            var metadata = candidate.Endpoint.Metadata.GetMetadata<ApiVersionMetadata>();

            if ( metadata == null )
            {
                continue;
            }

            // remember whether the candidate is currently valid. a matching api version will not
            // make the candidate valid; however, we want to short-circuit with 400 if no candidates
            // match the api version at all.
            switch ( metadata.MappingTo( apiVersion ) )
            {
                case Explicit:
                    bestMatches ??= new();
                    bestMatches.Add( i );
                    break;
                case Implicit:
                    implicitMatches ??= new();
                    implicitMatches.Add( i );
                    break;
            }

            // perf: always make the candidate invalid so we only need to loop through the
            // final, best matches for any remaining candidates
            candidates.SetValidity( i, false );
        }

        if ( bestMatches is null )
        {
            if ( implicitMatches is null )
            {
                return (false, hasCandidates);
            }

            for ( var i = 0; i < implicitMatches.Count; i++ )
            {
                candidates.SetValidity( implicitMatches[i], true );
            }

            return (true, hasCandidates);
        }

        if ( bestMatches.Count == 1 && implicitMatches is not null )
        {
            ref var candidate = ref candidates[bestMatches[0]];
            var metadata = candidate.Endpoint.Metadata.GetMetadata<ApiVersionMetadata>()!;

            if ( metadata.IsApiVersionNeutral )
            {
                for ( var i = 0; i < implicitMatches.Count; i++ )
                {
                    candidates.SetValidity( implicitMatches[i], true );
                }
            }
        }

        for ( var i = 0; i < bestMatches.Count; i++ )
        {
            candidates.SetValidity( bestMatches[i], true );
        }

        return (true, hasCandidates);
    }

    private ApiVersion TrySelectApiVersion( HttpContext httpContext, CandidateSet candidates )
    {
        var models = new List<ApiVersionModel>( capacity: candidates.Count );

        for ( var i = 0; i < candidates.Count; i++ )
        {
            if ( !candidates.IsValidCandidate( i ) )
            {
                continue;
            }

            ref var candidate = ref candidates[i];
            var metadata = candidate.Endpoint.Metadata.GetMetadata<ApiVersionMetadata>();

            if ( metadata != null )
            {
                models.Add( metadata.Map( Explicit ) );
            }
        }

        return ApiVersionSelector.SelectVersion( httpContext.Request, models.Aggregate() );
    }

    bool INodeBuilderPolicy.AppliesToEndpoints( IReadOnlyList<Endpoint> endpoints ) =>
        !ContainsDynamicEndpoints( endpoints ) && AppliesToEndpoints( endpoints );
}