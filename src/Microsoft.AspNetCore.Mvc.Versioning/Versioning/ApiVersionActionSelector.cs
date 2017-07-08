namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using ActionConstraints;
    using AspNetCore.Routing;
    using Extensions.Logging;
    using Extensions.Options;
    using Http;
    using Infrastructure;
    using Internal;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using static ErrorCodes;

    /// <summary>
    /// Represents the logic for selecting an API-versioned, action method.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersionActionSelector : IActionSelector
    {
        static readonly IReadOnlyList<ActionDescriptor> NoMatches = Array.Empty<ActionDescriptor>();
        readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
        readonly ActionConstraintCache actionConstraintCache;
        readonly IOptions<ApiVersioningOptions> options;
        Cache cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionActionSelector"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider "/> used to select candidate routes.</param>
        /// <param name="actionConstraintCache">The <see cref="ActionConstraintCache"/> that providers a set of <see cref="IActionConstraint"/> instances.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public ApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory )
        {
            Arg.NotNull( actionDescriptorCollectionProvider, nameof( actionDescriptorCollectionProvider ) );
            Arg.NotNull( actionConstraintCache, nameof( actionConstraintCache ) );
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( loggerFactory, nameof( loggerFactory ) );

            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            this.actionConstraintCache = actionConstraintCache;
            this.options = options;
            Logger = loggerFactory.CreateLogger( GetType() );
        }

        Cache Current
        {
            get
            {
                var actions = actionDescriptorCollectionProvider.ActionDescriptors;
                var value = Volatile.Read( ref cache );

                if ( value != null && value.Version == actions.Version )
                {
                    return value;
                }

                value = new Cache( actions );
                Volatile.Write( ref cache, value );

                return value;
            }
        }

        /// <summary>
        /// Gets the configuration options associated with the action selector.
        /// </summary>
        /// <value>The associated <see cref="ApiVersioningOptions">service versioning options</see>.</value>
        protected ApiVersioningOptions Options => options.Value;

        /// <summary>
        /// Gets the API version selector associated with the action selector.
        /// </summary>
        /// <value>The <see cref="IApiVersionSelector">API version selector</see> used to select the default
        /// <see cref="ApiVersion">API version</see> when a client does not specify a version.</value>
        protected IApiVersionSelector ApiVersionSelector => Options.ApiVersionSelector;

        /// <summary>
        /// Gets the logger associated with the action selector.
        /// </summary>
        /// <value>The associated <see cref="ILogger">logger</see>.</value>
        protected ILogger Logger { get; }

        /// <summary>
        /// Selects a list of candidate actions from the specified route context.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="ActionDescriptor">actions</see>.</returns>
        public virtual IReadOnlyList<ActionDescriptor> SelectCandidates( RouteContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            var cache = Current;
            var keys = cache.RouteKeys;
            var values = new string[keys.Length];

            for ( var i = 0; i < keys.Length; i++ )
            {
                context.RouteData.Values.TryGetValue( keys[i], out object value );

                if ( value != null )
                {
                    values[i] = value as string ?? Convert.ToString( value );
                }
            }

            if ( cache.OrdinalEntries.TryGetValue( values, out var matchingRouteValues ) ||
                 cache.OrdinalIgnoreCaseEntries.TryGetValue( values, out matchingRouteValues ) )
            {
                return matchingRouteValues;
            }

            Logger.NoActionsMatched( context.RouteData.Values );
            return NoMatches;
        }

        /// <summary>
        /// Selects the best action given the provided route context and list of candidate actions.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see> to evaluate.</param>
        /// <param name="candidates">The <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="ActionDescriptor">actions</see> to select from.</param>
        /// <returns>The best candidate <see cref="ActionDescriptor">action</see> or <c>null</c> if no candidate matches.</returns>
        public virtual ActionDescriptor SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( candidates, nameof( candidates ) );

            var httpContext = context.HttpContext;

            if ( ( context.Handler = VerifyRequestedApiVersionIsNotAmbiguous( httpContext, out var apiVersion ) ) != null )
            {
                return null;
            }

            var matches = EvaluateActionConstraints( context, candidates );
            var selectionContext = new ActionSelectionContext( httpContext, matches, apiVersion );
            var finalMatches = SelectBestActions( selectionContext );
            var properties = httpContext.ApiVersionProperties();
            var selectionResult = properties.SelectionResult;

            properties.ApiVersion = selectionContext.RequestedVersion;
            selectionResult.AddCandidates( candidates );

            if ( finalMatches == null )
            {
                selectionResult.EndIteration();
                return null;
            }

            if ( finalMatches.Count == 1 )
            {
                var selectedAction = finalMatches[0];

                // note: short-circuit if the api version policy has already been applied to the match
                // and there are no other matches in a previous iteration which would take precendence
                if ( selectedAction.VersionPolicyIsApplied() && !selectionResult.HasMatchesInPreviousIterations )
                {
                    httpContext.ApiVersionProperties().ApiVersion = selectionContext.RequestedVersion;
                    return selectedAction;
                }
            }

            if ( finalMatches.Count > 0 )
            {
                var routeData = new RouteData( context.RouteData );
                var matchingActions = new MatchingActionSequence( finalMatches, routeData );

                selectionResult.AddMatches( matchingActions );
                selectionResult.TrySetBestMatch( matchingActions.BestMatch );
            }

            // note: even though we may have had a successful match, this method could be called multiple times. the final decision
            // is made by the IApiVersionRoutePolicy. we return here to make sure all candidates have been considered at least once.
            selectionResult.EndIteration();
            return null;
        }

        /// <summary>
        /// Returns the set of best matching actions.
        /// </summary>
        /// <param name="context">The <see cref="ActionSelectionContext">context</see> to select the actions from.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of the best matching <see cref="ActionDescriptor">actions</see>.</returns>
        protected virtual IReadOnlyList<ActionDescriptor> SelectBestActions( ActionSelectionContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            var bestMatches = new List<ActionDescriptor>( context.MatchingActions.Count );

            bestMatches.AddRange( MatchVersionNeutralActions( context ) );

            if ( context.RequestedVersion == null )
            {
                if ( !Options.AssumeDefaultVersionWhenUnspecified )
                {
                    return bestMatches;
                }

                context.RequestedVersion = ApiVersionSelector.SelectVersion( context.HttpContext.Request, context.AllVersions );

                if ( context.RequestedVersion == null )
                {
                    return bestMatches;
                }
            }

            var implicitMatches = new List<ActionDescriptor>();
            var explicitMatches = from action in context.MatchingActions
                                  let model = action.GetProperty<ApiVersionModel>()
                                  where ActionIsSatisfiedBy( action, model, context.RequestedVersion, implicitMatches )
                                  select action;

            bestMatches.AddRange( explicitMatches );

            if ( bestMatches.Count == 0 )
            {
                bestMatches.AddRange( implicitMatches );
            }

            if ( bestMatches.Count != 1 )
            {
                return bestMatches;
            }

            if ( bestMatches[0].IsApiVersionNeutral() )
            {
                bestMatches.AddRange( implicitMatches );
            }

            return bestMatches;
        }

        RequestHandler VerifyRequestedApiVersionIsNotAmbiguous( HttpContext httpContext, out ApiVersion apiVersion )
        {
            Contract.Requires( httpContext != null );

            try
            {
                apiVersion = httpContext.GetRequestedApiVersion();
            }
            catch ( AmbiguousApiVersionException ex )
            {
                Logger.LogInformation( ex.Message );
                apiVersion = default( ApiVersion );
                return new BadRequestHandler( Options.ErrorResponses, AmbiguousApiVersion, ex.Message );
            }

            return null;
        }

        static IEnumerable<ActionDescriptor> MatchVersionNeutralActions( ActionSelectionContext context ) =>
            from action in context.MatchingActions
            let model = action.GetProperty<ApiVersionModel>()
            where model?.IsApiVersionNeutral ?? false
            select action;

        static bool ActionIsSatisfiedBy( ActionDescriptor action, ApiVersionModel model, ApiVersion version, ICollection<ActionDescriptor> implicitMatches )
        {
            Contract.Requires( action != null );
            Contract.Requires( implicitMatches != null );

            if ( model == null )
            {
                return false;
            }

            if ( action.IsMappedTo( version ) )
            {
                return true;
            }

            if ( action.IsImplicitlyMappedTo( version ) )
            {
                implicitMatches.Add( action );
            }

            return false;
        }

        IReadOnlyList<ActionDescriptor> EvaluateActionConstraints( RouteContext context, IReadOnlyList<ActionDescriptor> actions )
        {
            Contract.Requires( context != null );
            Contract.Requires( actions != null );
            Contract.Ensures( Contract.Result<IReadOnlyList<ActionDescriptor>>() != null );

            var candidates = new List<ActionSelectorCandidate>();

            for ( var i = 0; i < actions.Count; i++ )
            {
                var action = actions[i];
                var constraints = actionConstraintCache.GetActionConstraints( context.HttpContext, action );
                candidates.Add( new ActionSelectorCandidate( action, constraints ) );
            }

            var matches = EvaluateActionConstraintsCore( context, candidates, startingOrder: null );

            if ( matches == null )
            {
                return NoMatches;
            }

            return matches.Select( candidate => candidate.Action ).ToArray();
        }

        IReadOnlyList<ActionSelectorCandidate> EvaluateActionConstraintsCore( RouteContext context, IReadOnlyList<ActionSelectorCandidate> candidates, int? startingOrder )
        {
            Contract.Requires( context != null );
            Contract.Requires( candidates != null );

            var order = default( int? );

            for ( var i = 0; i < candidates.Count; i++ )
            {
                var candidate = candidates[i];

                if ( candidate.Constraints == null )
                {
                    continue;
                }

                for ( var j = 0; j < candidate.Constraints.Count; j++ )
                {
                    var constraint = candidate.Constraints[j];

                    if ( ( startingOrder == null || constraint.Order > startingOrder ) && ( order == null || constraint.Order < order ) )
                    {
                        order = constraint.Order;
                    }
                }
            }

            if ( order == null )
            {
                return candidates;
            }

            var actionsWithConstraint = new List<ActionSelectorCandidate>();
            var actionsWithoutConstraint = new List<ActionSelectorCandidate>();
            var constraintContext = new ActionConstraintContext()
            {
                Candidates = candidates,
                RouteContext = context
            };

            for ( var i = 0; i < candidates.Count; i++ )
            {
                var candidate = candidates[i];
                var isMatch = true;
                var foundMatchingConstraint = false;

                if ( candidate.Constraints != null )
                {
                    constraintContext.CurrentCandidate = candidate;

                    for ( var j = 0; j < candidate.Constraints.Count; j++ )
                    {
                        var constraint = candidate.Constraints[j];

                        if ( constraint.Order != order )
                        {
                            continue;
                        }

                        foundMatchingConstraint = true;

                        if ( !constraint.Accept( constraintContext ) )
                        {
                            isMatch = false;
                            Logger.ConstraintMismatch( candidate.Action.DisplayName, candidate.Action.Id, constraint );
                            break;
                        }
                    }
                }

                if ( isMatch && foundMatchingConstraint )
                {
                    actionsWithConstraint.Add( candidate );
                }
                else if ( isMatch )
                {
                    actionsWithoutConstraint.Add( candidate );
                }
            }

            if ( actionsWithConstraint.Count > 0 )
            {
                var matches = EvaluateActionConstraintsCore( context, actionsWithConstraint, order );

                if ( matches?.Count > 0 )
                {
                    return matches;
                }
            }

            if ( actionsWithoutConstraint.Count == 0 )
            {
                return null;
            }
            else
            {
                return EvaluateActionConstraintsCore( context, actionsWithoutConstraint, order );
            }
        }

        sealed class MatchingActionSequence : IEnumerable<ActionDescriptorMatch>
        {
            readonly IReadOnlyList<ActionDescriptor> matches;
            readonly RouteData routeData;

            internal MatchingActionSequence( IReadOnlyList<ActionDescriptor> matches, RouteData routeData )
            {
                Contract.Requires( matches != null );
                Contract.Requires( matches.Count > 0 );
                Contract.Requires( routeData != null );

                this.matches = matches;
                this.routeData = routeData;
            }

            internal ActionDescriptorMatch BestMatch { get; private set; }

            RouteData NewMatchRouteData( ActionDescriptor match )
            {
                Contract.Requires( match != null );
                Contract.Ensures( Contract.Result<RouteData>() != null );

                var matchedRouteData = new RouteData( routeData );
                var matchedRouteValues = matchedRouteData.Values;

                foreach ( var entry in match.RouteValues )
                {
                    if ( !matchedRouteValues.ContainsKey( entry.Key ) )
                    {
                        matchedRouteValues.Add( entry.Key, entry.Value );
                    }
                }

                return matchedRouteData;
            }

            public IEnumerator<ActionDescriptorMatch> GetEnumerator()
            {
                if ( matches.Count == 1 )
                {
                    var match = matches[0];
                    BestMatch = new ActionDescriptorMatch( match, NewMatchRouteData( match ) );
                    yield return BestMatch;
                }
                else
                {
                    for ( var i = 0; i < matches.Count; i++ )
                    {
                        var match = matches[i];
                        yield return new ActionDescriptorMatch( match, NewMatchRouteData( match ) );
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        sealed class Cache
        {
            public Cache( ActionDescriptorCollection actions )
            {
                Contract.Requires( actions != null );

                Version = actions.Version;
                OrdinalEntries = new Dictionary<string[], List<ActionDescriptor>>(StringArrayComparer.Ordinal);
                OrdinalIgnoreCaseEntries = new Dictionary<string[], List<ActionDescriptor>>(StringArrayComparer.OrdinalIgnoreCase);
                RouteKeys = IdentifyRouteKeysForActionSelection( actions );
                BuildOrderedSetOfKeysForRouteValues( actions );
            }

            public int Version { get; }
            public string[] RouteKeys { get; }
            public Dictionary<string[], List<ActionDescriptor>> OrdinalEntries { get; }
            public Dictionary<string[], List<ActionDescriptor>> OrdinalIgnoreCaseEntries { get; }

            static string[] IdentifyRouteKeysForActionSelection( ActionDescriptorCollection actions )
            {
                Contract.Requires( actions != null );
                Contract.Ensures( Contract.Result<string[]>() != null );

                var routeKeys = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

                for ( var i = 0; i < actions.Items.Count; i++ )
                {
                    var action = actions.Items[i];

                    if ( action.AttributeRouteInfo == null )
                    {
                        foreach ( var kvp in action.RouteValues )
                        {
                            routeKeys.Add(kvp.Key);
                        }
                    }
                }

                return routeKeys.ToArray();
            }

            void BuildOrderedSetOfKeysForRouteValues( ActionDescriptorCollection actions )
            {
                Contract.Requires( actions != null );

                for ( var i = 0; i < actions.Items.Count; i++ )
                {
                    var action = actions.Items[i];

                    if ( action.AttributeRouteInfo != null )
                    {
                        continue;
                    }

                    var routeValues = new string[RouteKeys.Length];

                    for ( var j = 0; j < RouteKeys.Length; j++ )
                    {

                        action.RouteValues.TryGetValue(RouteKeys[j], out routeValues[j]);
                    }

                    if ( !OrdinalIgnoreCaseEntries.TryGetValue( routeValues, out var entries ) )
                    {
                        entries = new List<ActionDescriptor>();
                        OrdinalIgnoreCaseEntries.Add( routeValues, entries );
                    }

                    entries.Add(action);

                    if ( !OrdinalEntries.ContainsKey( routeValues ) )
                    {
                        OrdinalEntries.Add( routeValues, entries );
                    }
                }
            }
        }

        sealed class StringArrayComparer : IEqualityComparer<string[]>
        {
            readonly StringComparer valueComparer;
            public static readonly StringArrayComparer Ordinal = new StringArrayComparer( StringComparer.Ordinal );
            public static readonly StringArrayComparer OrdinalIgnoreCase = new StringArrayComparer( StringComparer.OrdinalIgnoreCase );

            StringArrayComparer( StringComparer valueComparer ) => this.valueComparer = valueComparer;

            public bool Equals( string[] x, string[] y )
            {
                if ( ReferenceEquals( x, y ) )
                {
                    return true;
                }

                if ( x == null ^ y == null )
                {
                    return false;
                }

                if ( x.Length != y.Length )
                {
                    return false;
                }

                for ( var i = 0; i < x.Length; i++ )
                {
                    if ( string.IsNullOrEmpty( x[i] ) && string.IsNullOrEmpty( y[i] ) )
                    {
                        continue;
                    }

                    if ( !valueComparer.Equals( x[i], y[i] ) )
                    {
                        return false;
                    }
                }

                return true;
            }

            public int GetHashCode( string[] obj )
            {
                if ( obj == null )
                {
                    return 0;
                }

                var hash = 0;
                var i = 0;

                for ( ; i < obj.Length; i++ )
                {
                    if ( obj[i] != null )
                    {
                        hash = valueComparer.GetHashCode( obj[i] );
                        break;
                    }
                }

                for ( ; i < obj.Length; i++ )
                {
                    if ( obj[i] != null )
                    {
                        hash = ( hash * 397 ) ^ valueComparer.GetHashCode( obj[i] );
                    }
                }

                return hash;
            }
        }
    }
}