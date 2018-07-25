namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using static ErrorCodes;
    using static System.Globalization.CultureInfo;

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
        /// <param name="routePolicy">The <see cref="IApiVersionRoutePolicy">route policy</see> applied to candidate matches.</param>
        public ApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy )
        {
            Arg.NotNull( actionDescriptorCollectionProvider, nameof( actionDescriptorCollectionProvider ) );
            Arg.NotNull( actionConstraintCache, nameof( actionConstraintCache ) );
            Arg.NotNull( options, nameof( options ) );
            Arg.NotNull( loggerFactory, nameof( loggerFactory ) );
            Arg.NotNull( routePolicy, nameof( routePolicy ) );

            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            this.actionConstraintCache = actionConstraintCache;
            this.options = options;
            Logger = loggerFactory.CreateLogger( GetType() );
            RoutePolicy = routePolicy;
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
        /// Gets the route policy applied to selected candidate actions.
        /// </summary>
        /// <value>The <see cref="IApiVersionRoutePolicy">route policy</see> applied to selected candidate actions.</value>
        protected IApiVersionRoutePolicy RoutePolicy { get; }

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
                context.RouteData.Values.TryGetValue( keys[i], out var value );

                if ( value != null )
                {
                    values[i] = value as string ?? Convert.ToString( value, InvariantCulture );
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

            if ( IsRequestedApiVersionAmbiguous( context, out var apiVersion ) )
            {
                return null;
            }

            var matches = EvaluateActionConstraints( context, candidates );
            var selectedAction = SelectActionWithoutApiVersionConvention( matches );

            if ( selectedAction != null )
            {
                return selectedAction;
            }

            var selectionContext = new ActionSelectionContext( httpContext, matches, apiVersion );
            var finalMatches = SelectBestActions( selectionContext );
            var feature = httpContext.Features.Get<IApiVersioningFeature>();
            var selectionResult = feature.SelectionResult;

            feature.RequestedApiVersion = selectionContext.RequestedVersion;
            selectionResult.AddCandidates( candidates );

            if ( finalMatches.Count == 0 )
            {
                return null;
            }

            selectionResult.AddMatches( finalMatches );

            return RoutePolicy.Evaluate( context, selectionResult );
        }

        /// <summary>
        /// Returns the set of best matching actions.
        /// </summary>
        /// <param name="context">The <see cref="ActionSelectionContext">context</see> to select the actions from.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of the best matching <see cref="ActionDescriptor">actions</see>.</returns>
        protected virtual IReadOnlyList<ActionDescriptor> SelectBestActions( ActionSelectionContext context )
        {
            Arg.NotNull( context, nameof( context ) );
            Contract.Ensures( Contract.Result<IReadOnlyList<ActionDescriptor>>() != null );

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

        /// <summary>
        /// Selects a candidate action which does not have the API versioning convention applied.
        /// </summary>
        /// <param name="matches">The current <see cref="IReadOnlyList{T}">read-only list</see> of
        /// matching <see cref="ActionDescriptor">action</see>.</param>
        /// <returns>The selected <see cref="ActionDescriptor">action</see> with the API versioning convention applied or <c>null</c>.</returns>
        /// <remarks>This method typically matches a non-API action such as a Razor page.</remarks>
        protected virtual ActionDescriptor SelectActionWithoutApiVersionConvention( IReadOnlyList<ActionDescriptor> matches )
        {
            Arg.NotNull( matches, nameof( matches ) );

            if ( matches.Count != 1 )
            {
                return null;
            }

            var selectedAction = matches[0];

            if ( selectedAction.GetProperty<ApiVersionModel>() == null )
            {
                return selectedAction;
            }

            return null;
        }

        /// <summary>
        /// Verifies the requested API version is not ambiguous.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see>.</param>
        /// <param name="apiVersion">The requested <see cref="ApiVersion">API version</see> or <c>null</c>.</param>
        /// <returns>True if the requested API version is ambiguous; otherwise, false.</returns>
        /// <remarks>This method will also change the <see cref="RouteContext.Handler"/> to an appropriate
        /// error response if the API version is ambiguous.</remarks>
        protected virtual bool IsRequestedApiVersionAmbiguous( RouteContext context, out ApiVersion apiVersion )
        {
            Arg.NotNull( context, nameof( context ) );

            var httpContext = context.HttpContext;

            try
            {
                apiVersion = httpContext.GetRequestedApiVersion();
            }
            catch ( AmbiguousApiVersionException ex )
            {
                Logger.LogInformation( ex.Message );
                apiVersion = default;

                var handlerContext = new RequestHandlerContext( Options.ErrorResponses )
                {
                    Code = AmbiguousApiVersion,
                    Message = ex.Message,
                };

                context.Handler = new BadRequestHandler( handlerContext );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a filtered list of actions based on the evaluated action constraints.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see>.</param>
        /// <param name="actions">The <see cref="IReadOnlyList{T}">read-only list</see> of <see cref="ActionDescriptor">actions</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of the remaining <see cref="ActionDescriptor">actions</see> after all
        /// action constraints have been evaluated.</returns>
        protected virtual IReadOnlyList<ActionDescriptor> EvaluateActionConstraints( RouteContext context, IReadOnlyList<ActionDescriptor> actions )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( actions, nameof( actions ) );
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
                RouteContext = context,
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

#pragma warning disable CA1724 // private type and will not cause a conflict
        sealed class Cache
#pragma warning restore CA1724
        {
            public Cache( ActionDescriptorCollection actions )
            {
                Contract.Requires( actions != null );

                Version = actions.Version;
                RouteKeys = IdentifyRouteKeysForActionSelection( actions );
                BuildOrderedSetOfKeysForRouteValues( actions );
            }

            public int Version { get; }

            public string[] RouteKeys { get; }

            public Dictionary<string[], List<ActionDescriptor>> OrdinalEntries { get; } = new Dictionary<string[], List<ActionDescriptor>>( StringArrayComparer.Ordinal );

            public Dictionary<string[], List<ActionDescriptor>> OrdinalIgnoreCaseEntries { get; } = new Dictionary<string[], List<ActionDescriptor>>( StringArrayComparer.OrdinalIgnoreCase );

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
                            routeKeys.Add( kvp.Key );
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
                        action.RouteValues.TryGetValue( RouteKeys[j], out routeValues[j] );
                    }

                    if ( !OrdinalIgnoreCaseEntries.TryGetValue( routeValues, out var entries ) )
                    {
                        entries = new List<ActionDescriptor>();
                        OrdinalIgnoreCaseEntries.Add( routeValues, entries );
                    }

                    entries.Add( action );

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