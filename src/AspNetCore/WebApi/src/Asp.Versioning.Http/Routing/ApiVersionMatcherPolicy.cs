// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Buffers;
using System.Collections.Frozen;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static Asp.Versioning.ApiVersionMapping;
using static System.Text.RegularExpressions.RegexOptions;

/// <summary>
/// Represents the <see cref="MatcherPolicy">matcher policy</see> for API versions.
/// </summary>
[CLSCompliant( false )]
public sealed partial class ApiVersionMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy, INodeBuilderPolicy
{
    private readonly IOptions<ApiVersioningOptions> options;
    private readonly IApiVersionParser apiVersionParser;
    private readonly ApiVersionCollator collator;
    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersionMatcherPolicy"/> class.
    /// </summary>
    /// <param name="apiVersionParser">The <see cref="IApiVersionParser">parser</see> used to parse API versions.</param>
    /// <param name="providers">The <see cref="IEnumerable{T}">sequence</see> of
    /// <see cref="IApiVersionMetadataCollationProvider">API version metadata collation providers.</see>.</param>
    /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the matcher policy.</param>
    /// <param name="logger">The <see cref="ILogger{T}">logger</see> used by the matcher policy.</param>
    public ApiVersionMatcherPolicy(
        IApiVersionParser apiVersionParser,
        IEnumerable<IApiVersionMetadataCollationProvider> providers,
        IOptions<ApiVersioningOptions> options,
        ILogger<ApiVersionMatcherPolicy> logger )
    {
        ArgumentNullException.ThrowIfNull( apiVersionParser );
        ArgumentNullException.ThrowIfNull( providers );
        ArgumentNullException.ThrowIfNull( options );
        ArgumentNullException.ThrowIfNull( logger );

        this.apiVersionParser = apiVersionParser;
        collator = new( providers, options );
        this.options = options;
        this.logger = logger;
    }

    /// <inheritdoc />
    public override int Order { get; } = BeforeDefaultMatcherPolicy();

    private ApiVersioningOptions Options => options.Value;

    private IApiVersionParameterSource ApiVersionSource => options.Value.ApiVersionReader;

    private IApiVersionSelector ApiVersionSelector => options.Value.ApiVersionSelector;

    /// <inheritdoc />
    public bool AppliesToEndpoints( IReadOnlyList<Endpoint> endpoints )
    {
        ArgumentNullException.ThrowIfNull( endpoints );

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
    public async Task ApplyAsync( HttpContext httpContext, CandidateSet candidates )
    {
        ArgumentNullException.ThrowIfNull( httpContext );
        ArgumentNullException.ThrowIfNull( candidates );

        var feature = httpContext.ApiVersioningFeature();
        var apiVersion = feature.RequestedApiVersion;

        if ( apiVersion == null && Options.AssumeDefaultVersionWhenUnspecified )
        {
            apiVersion = await TrySelectApiVersionAsync( httpContext, candidates ).ConfigureAwait( false );
            feature.RequestedApiVersion = apiVersion;
        }

        var (matched, hasCandidates) = MatchApiVersion( candidates, apiVersion );

        if ( !matched && hasCandidates && !DifferByRouteConstraintsOnly( candidates ) )
        {
            var builder = new ClientErrorEndpointBuilder( feature, candidates, Options, logger );
            httpContext.SetEndpoint( builder.Build() );
        }
    }

    /// <inheritdoc />
    public PolicyJumpTable BuildJumpTable( int exitDestination, IReadOnlyList<PolicyJumpTableEdge> edges )
    {
        ArgumentNullException.ThrowIfNull( edges );

        var rejection = new RouteDestination( exitDestination );
        var capacity = edges.Count - EdgeBuilder.NumberOfRejectionEndpoints;
        var destinations = new Dictionary<ApiVersion, int>( capacity );
        var source = ApiVersionSource;
        var supported = default( SortedSet<ApiVersion> );
        var deprecated = default( SortedSet<ApiVersion> );
        var routePatterns = default( RoutePattern[] );

        for ( var i = 0; i < edges.Count; i++ )
        {
            var edge = edges[i];
            var state = (EdgeKey) edge.State;

            if ( Options.ReportApiVersions )
            {
                Collate( state.Metadata, ref supported, ref deprecated );
            }

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
                case EndpointType.Unsupported:
                    rejection.Unsupported = edge.Destination;
                    break;
                case EndpointType.UnsupportedMediaType:
                    rejection.UnsupportedMediaType = edge.Destination;
                    break;
                case EndpointType.AssumeDefault:
                    rejection.AssumeDefault = edge.Destination;
                    break;
                case EndpointType.NotAcceptable:
                    rejection.NotAcceptable = edge.Destination;
                    break;
                default:
                    // the route patterns provided to each edge is a
                    // singleton so any edge will do
                    routePatterns ??= [.. state.RoutePatterns];
                    destinations.Add( state.ApiVersion, edge.Destination );
                    break;
            }
        }

        return new ApiVersionPolicyJumpTable(
            rejection,
            destinations.ToFrozenDictionary( destinations.Comparer ),
            NewPolicyFeature( supported, deprecated ),
            routePatterns ?? [],
            apiVersionParser,
            source,
            Options );
    }

    /// <inheritdoc />
    public IReadOnlyList<PolicyNodeEdge> GetEdges( IReadOnlyList<Endpoint> endpoints )
    {
        ArgumentNullException.ThrowIfNull( endpoints );

        var capacity = endpoints.Count;
        var builder = new EdgeBuilder( capacity, ApiVersionSource, Options, logger );
        var versions = new SortedSet<ApiVersion>();
        var neutralEndpoints = default( List<(RouteEndpoint, ApiVersionMetadata)> );
        var versionedEndpoints = new (RouteEndpoint, ApiVersionModel, ApiVersionMetadata)[capacity];
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
                builder.Add( endpoint, ApiVersion.Neutral, metadata );
                neutralEndpoints ??= [];
                neutralEndpoints.Add( (endpoint, metadata) );
            }
            else
            {
                builder.Add( endpoint );
                versionedEndpoints[count++] = (endpoint, model, metadata);
                versions.AddRange( model.DeclaredApiVersions );
            }
        }

        foreach ( var version in versions )
        {
            for ( var j = 0; j < count; j++ )
            {
                var (endpoint, model, metadata) = versionedEndpoints[j];
                var mappedWithImplementation = model.ImplementedApiVersions.Contains( version );

                if ( mappedWithImplementation )
                {
                    builder.Add( endpoint, version, metadata );
                }
            }
        }

        if ( neutralEndpoints != null )
        {
            var allVersions = collator.Items;

            // add an edge for all known versions because version-neutral endpoints can map to any api version
            for ( var i = 0; i < neutralEndpoints.Count; i++ )
            {
                var (endpoint, metadata) = neutralEndpoints[i];

                for ( var j = 0; j < allVersions.Count; j++ )
                {
                    builder.Add( endpoint, allVersions[j], metadata );
                }
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
        var pattern = RouteConstraintRegex();
        var comparer = StringComparer.OrdinalIgnoreCase;
        string? template = default;
        string? normalizedTemplate = default;

        for ( var i = 0; i < candidates.Count; i++ )
        {
            ref readonly var candidate = ref candidates[i];

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

    private static void Collate(
        ApiVersionMetadata metadata,
        ref SortedSet<ApiVersion>? supported,
        ref SortedSet<ApiVersion>? deprecated )
    {
        var model = metadata.Map( Implicit | Explicit );
        var versions = model.SupportedApiVersions;

        if ( versions.Count > 0 )
        {
            supported ??= [];

            for ( var j = 0; j < versions.Count; j++ )
            {
                supported.Add( versions[j] );
            }
        }

        versions = model.DeprecatedApiVersions;

        if ( versions.Count == 0 )
        {
            return;
        }

        deprecated ??= [];

        for ( var j = 0; j < versions.Count; j++ )
        {
            deprecated.Add( versions[j] );
        }
    }

    private static ApiVersionPolicyFeature? NewPolicyFeature(
        SortedSet<ApiVersion>? supported,
        SortedSet<ApiVersion>? deprecated )
    {
        // this is a best guess effort at collating all supported and deprecated
        // versions for an api when unmatched and it needs to be reported. it's
        // impossible to sure as there is no way to correlate an arbitrary
        // request url by endpoint or name. the routing system will build a tree
        // based on the route template before the jump table policy is created,
        // which provides a natural method of grouping. manual, contrived tests
        // demonstrated that were the results were correctly collated together.
        // it is possible there is an edge case that isn't covered, but it's
        // unclear what that would look like. one or more test cases should be
        // added to document that if discovered
        ApiVersionModel model;

        if ( supported == null )
        {
            if ( deprecated == null )
            {
                return default;
            }

            model = new( Enumerable.Empty<ApiVersion>(), deprecated );
        }
        else if ( deprecated == null )
        {
            model = new( supported, Enumerable.Empty<ApiVersion>() );
        }
        else
        {
            deprecated.ExceptWith( supported );
            model = new( supported, deprecated );
        }

        return new( new( model, model ) );
    }

    private static (bool Matched, bool HasCandidates) MatchApiVersion( CandidateSet candidates, ApiVersion? apiVersion )
    {
        var total = candidates.Count;
        var count = 0;
        var array = default( Match[] );
        var bestMatch = default( Match? );
        var hasCandidates = false;
        Span<Match> matches =
            total <= 16
            ? stackalloc Match[total]
            : ( array = ArrayPool<Match>.Shared.Rent( total ) ).AsSpan();

        for ( var i = 0; i < total; i++ )
        {
            if ( !candidates.IsValidCandidate( i ) )
            {
                continue;
            }

            hasCandidates = true;
            ref readonly var candidate = ref candidates[i];
            var metadata = candidate.Endpoint.Metadata.GetMetadata<ApiVersionMetadata>();

            if ( metadata == null )
            {
                continue;
            }

            var score = candidate.Score;
            bool isExplicit;

            // perf: always make the candidate invalid so we only need to loop through the
            // final, best matches for any remaining candidates
            candidates.SetValidity( i, false );

            switch ( metadata.MappingTo( apiVersion ) )
            {
                case Explicit:
                    isExplicit = true;
                    break;
                case Implicit:
                    isExplicit = metadata.IsApiVersionNeutral;
                    break;
                default:
                    continue;
            }

            var match = new Match( i, score, isExplicit );

            matches[count++] = match;

            if ( !bestMatch.HasValue || match.CompareTo( bestMatch.Value ) > 0 )
            {
                bestMatch = match;
            }
        }

        var matched = false;

        if ( bestMatch.HasValue )
        {
            matched = true;
            var match = bestMatch.Value;

            for ( var i = 0; i < count; i++ )
            {
                ref readonly var otherMatch = ref matches[i];

                if ( match.CompareTo( otherMatch ) == 0 )
                {
                    candidates.SetValidity( otherMatch.Index, true );
                }
            }
        }

        if ( array is not null )
        {
            ArrayPool<Match>.Shared.Return( array );
        }

        return (matched, hasCandidates);
    }

    private ValueTask<ApiVersion> TrySelectApiVersionAsync( HttpContext httpContext, CandidateSet candidates )
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

        return ApiVersionSelector.SelectVersionAsync(
            httpContext.Request,
            models.Aggregate(),
            httpContext.RequestAborted );
    }

    bool INodeBuilderPolicy.AppliesToEndpoints( IReadOnlyList<Endpoint> endpoints ) =>
        !ContainsDynamicEndpoints( endpoints ) && AppliesToEndpoints( endpoints );

    private readonly struct Match( int index, int score, bool isExplicit )
    {
        internal readonly int Index = index;
        internal readonly int Score = score;
        internal readonly bool IsExplicit = isExplicit;

        internal int CompareTo( in Match other )
        {
            var result = -Score.CompareTo( other.Score );
            return result == 0 ? IsExplicit.CompareTo( other.IsExplicit ) : result;
        }
    }

    private sealed class ApiVersionCollator(
            IEnumerable<IApiVersionMetadataCollationProvider> providers,
            IOptions<ApiVersioningOptions> options )
    {
        private readonly IApiVersionMetadataCollationProvider[] providers = providers.ToArray();
        private readonly object syncRoot = new();
        private IReadOnlyList<ApiVersion>? items;
        private int version;

        public IReadOnlyList<ApiVersion> Items
        {
            get
            {
                if ( items is not null && version == ComputeVersion() )
                {
                    return items;
                }

                lock ( syncRoot )
                {
                    var currentVersion = ComputeVersion();

                    if ( items is not null && version == currentVersion )
                    {
                        return items;
                    }

                    var context = new ApiVersionMetadataCollationContext();

                    for ( var i = 0; i < providers.Length; i++ )
                    {
                        providers[i].Execute( context );
                    }

                    var results = context.Results;
                    var versions = new SortedSet<ApiVersion>();

                    for ( var i = 0; i < results.Count; i++ )
                    {
                        var model = results[i].Map( Explicit | Implicit );
                        var declared = model.DeclaredApiVersions;

                        for ( var j = 0; j < declared.Count; j++ )
                        {
                            versions.Add( declared[j] );
                        }
                    }

                    if ( versions.Count == 0 )
                    {
                        versions.Add( options.Value.DefaultApiVersion );
                    }

                    items = versions.ToArray();
                    version = currentVersion;
                }

                return items;
            }
        }

        private int ComputeVersion() =>
            providers.Length switch
            {
                0 => 0,
                1 => providers[0].Version,
                _ => ComputeVersion( providers ),
            };

        private static int ComputeVersion( IApiVersionMetadataCollationProvider[] providers )
        {
            var hash = default( HashCode );

            for ( var i = 0; i < providers.Length; i++ )
            {
                hash.Add( providers[i].Version );
            }

            return hash.ToHashCode();
        }
    }

    [GeneratedRegex( "{([^:]+):[^}]+}", IgnoreCase | Singleline )]
    private static partial Regex RouteConstraintRegex();
}