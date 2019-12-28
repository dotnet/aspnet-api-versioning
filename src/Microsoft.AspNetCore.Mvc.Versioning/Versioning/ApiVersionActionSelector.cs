namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using static ApiVersionMapping;
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
        readonly IOptions<ApiVersioningOptions> options;
        readonly ActionConstraintCache actionConstraintCache;
        ActionSelectionTable<ActionDescriptor>? cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionActionSelector"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider "/> used to select candidate routes.</param>
        /// <param name="actionConstraintProviders">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IActionConstraintProvider">action constraint providers</see>.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="routePolicy">The <see cref="IApiVersionRoutePolicy">route policy</see> applied to candidate matches.</param>
        public ApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IEnumerable<IActionConstraintProvider> actionConstraintProviders,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy )
        {
            this.actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
            actionConstraintCache = new ActionConstraintCache( actionDescriptorCollectionProvider, actionConstraintProviders );
            this.options = options;
            Logger = loggerFactory.CreateLogger( GetType() );
            RoutePolicy = routePolicy;
        }

        ActionSelectionTable<ActionDescriptor> Current
        {
            get
            {
                var actions = actionDescriptorCollectionProvider.ActionDescriptors;
                var value = Volatile.Read( ref cache );

                if ( value != null && value.Version == actions.Version )
                {
                    return value;
                }

                value = ActionSelectionTable<ActionDescriptor>.Create( actions );
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
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var cache = Current;
            var keys = cache.RouteKeys;
            var values = new string[keys.Length];
            var routeValues = context.RouteData.Values;

            for ( var i = 0; i < keys.Length; i++ )
            {
                routeValues.TryGetValue( keys[i], out var value );
                values[i] = value as string ?? Convert.ToString( value, InvariantCulture ) ?? string.Empty;
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
        public virtual ActionDescriptor? SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

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
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var requestedVersion = context.RequestedVersion;

            if ( requestedVersion == null )
            {
                if ( Options.AssumeDefaultVersionWhenUnspecified )
                {
                    context.RequestedVersion = requestedVersion = ApiVersionSelector.SelectVersion( context.HttpContext.Request, context.AllVersions );
                }
            }

            var bestMatches = new HashSet<ActionDescriptor>();
            var implicitMatches = new HashSet<ActionDescriptor>();

            for ( var i = 0; i < context.MatchingActions.Count; i++ )
            {
                var action = context.MatchingActions[i];

                switch ( action.MappingTo( requestedVersion ) )
                {
                    case Explicit:
                        bestMatches.Add( action );
                        break;
                    case Implicit:
                        implicitMatches.Add( action );
                        break;
                }
            }

            if ( bestMatches.Count == 0 )
            {
                bestMatches.AddRange( implicitMatches );
            }

            if ( bestMatches.Count == 1 )
            {
                var model = bestMatches.Single().GetApiVersionModel();

                if ( model.IsApiVersionNeutral )
                {
                    bestMatches.AddRange( implicitMatches );
                }
            }

            return bestMatches.ToArray();
        }

        /// <summary>
        /// Selects a candidate action which does not have the API versioning convention applied.
        /// </summary>
        /// <param name="matches">The current <see cref="IReadOnlyList{T}">read-only list</see> of
        /// matching <see cref="ActionDescriptor">action</see>.</param>
        /// <returns>The selected <see cref="ActionDescriptor">action</see> with the API versioning convention applied or <c>null</c>.</returns>
        /// <remarks>This method typically matches a non-API action such as a Razor page.</remarks>
        protected virtual ActionDescriptor? SelectActionWithoutApiVersionConvention( IReadOnlyList<ActionDescriptor> matches )
        {
            if ( matches == null )
            {
                throw new ArgumentNullException( nameof( matches ) );
            }

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
        protected virtual bool IsRequestedApiVersionAmbiguous( RouteContext context, out ApiVersion? apiVersion )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

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
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            if ( actions == null )
            {
                throw new ArgumentNullException( nameof( actions ) );
            }

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

        IReadOnlyList<ActionSelectorCandidate>? EvaluateActionConstraintsCore( RouteContext context, IReadOnlyList<ActionSelectorCandidate> candidates, int? startingOrder )
        {
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

        // REF: https://raw.githubusercontent.com/aspnet/AspNetCore/master/src/Mvc/Mvc.Core/src/Infrastructure/ActionSelectionTable.cs
        sealed class ActionSelectionTable<TItem>
        {
            ActionSelectionTable(
                int version,
                string[] routeKeys,
                Dictionary<string[], List<TItem>> ordinalEntries,
                Dictionary<string[], List<TItem>> ordinalIgnoreCaseEntries )
            {
                Version = version;
                RouteKeys = routeKeys;
                OrdinalEntries = ordinalEntries;
                OrdinalIgnoreCaseEntries = ordinalIgnoreCaseEntries;
            }

            public int Version { get; }

            internal string[] RouteKeys { get; }

            internal Dictionary<string[], List<TItem>> OrdinalEntries { get; }

            internal Dictionary<string[], List<TItem>> OrdinalIgnoreCaseEntries { get; }

            public static ActionSelectionTable<ActionDescriptor> Create( ActionDescriptorCollection actions )
            {
                return CreateCore(
                    version: actions.Version,
                    items: actions.Items.Where( a => a.AttributeRouteInfo == null ),
                    getRouteKeys: a => a.RouteValues.Keys,
                    getRouteValue: ( a, key ) =>
                    {
                        a.RouteValues.TryGetValue( key, out var value );
                        return value ?? string.Empty;
                    } );
            }

            public static ActionSelectionTable<Endpoint> Create( IEnumerable<Endpoint> endpoints )
            {
                return CreateCore(
                    version: 0,
                    items: endpoints.Where( e => e.GetType() == typeof( Endpoint ) ),
                    getRouteKeys: e => e.Metadata.GetMetadata<ActionDescriptor>().RouteValues.Keys,
                    getRouteValue: ( e, key ) =>
                    {
                        e.Metadata.GetMetadata<ActionDescriptor>().RouteValues.TryGetValue( key, out var value );
                        return Convert.ToString( value, InvariantCulture ) ?? string.Empty;
                    } );
            }

            static ActionSelectionTable<T> CreateCore<T>(
                int version,
                IEnumerable<T> items,
                Func<T, IEnumerable<string>> getRouteKeys,
                Func<T, string, string> getRouteValue )
            {
                var ordinalEntries = new Dictionary<string[], List<T>>( StringArrayComparer.Ordinal );
                var ordinalIgnoreCaseEntries = new Dictionary<string[], List<T>>( StringArrayComparer.OrdinalIgnoreCase );
                var routeKeys = new SortedSet<string>( StringComparer.OrdinalIgnoreCase );

                foreach ( var item in items )
                {
                    foreach ( var key in getRouteKeys( item ) )
                    {
                        routeKeys.Add( key );
                    }
                }

                foreach ( var item in items )
                {
                    var index = 0;
                    var routeValues = new string[routeKeys.Count];

                    foreach ( var key in routeKeys )
                    {
                        var value = getRouteValue( item, key );
                        routeValues[index++] = value;
                    }

                    if ( !ordinalIgnoreCaseEntries.TryGetValue( routeValues, out var entries ) )
                    {
                        entries = new List<T>();
                        ordinalIgnoreCaseEntries.Add( routeValues, entries );
                    }

                    entries.Add( item );

                    if ( !ordinalEntries.ContainsKey( routeValues ) )
                    {
                        ordinalEntries.Add( routeValues, entries );
                    }
                }

                return new ActionSelectionTable<T>( version, routeKeys.ToArray(), ordinalEntries, ordinalIgnoreCaseEntries );
            }
        }

        // REF: https://github.com/aspnet/AspNetCore/blob/master/src/Mvc/Mvc.Core/src/ActionConstraints/ActionConstraintCache.cs
        sealed class ActionConstraintCache
        {
            readonly IActionDescriptorCollectionProvider collectionProvider;
            readonly IActionConstraintProvider[] actionConstraintProviders;
            volatile InnerCache? currentCache;

            public ActionConstraintCache(
                IActionDescriptorCollectionProvider collectionProvider,
                IEnumerable<IActionConstraintProvider> actionConstraintProviders )
            {
                this.collectionProvider = collectionProvider;
                this.actionConstraintProviders = actionConstraintProviders.OrderBy( item => item.Order ).ToArray();
            }

            internal InnerCache CurrentCache
            {
                get
                {
                    var current = currentCache;
                    var actionDescriptors = collectionProvider.ActionDescriptors;

                    if ( current == null || current.Version != actionDescriptors.Version )
                    {
                        current = new InnerCache( actionDescriptors );
                        currentCache = current;
                    }

                    return current;
                }
            }

            public IReadOnlyList<IActionConstraint>? GetActionConstraints( HttpContext httpContext, ActionDescriptor action )
            {
                var cache = CurrentCache;

                if ( cache.Entries.TryGetValue( action, out var entry ) )
                {
                    return GetActionConstraintsFromEntry( entry, httpContext, action );
                }

                if ( action.ActionConstraints == null || action.ActionConstraints.Count == 0 )
                {
                    return null;
                }

                var items = new List<ActionConstraintItem>( action.ActionConstraints.Count );

                for ( var i = 0; i < action.ActionConstraints.Count; i++ )
                {
                    items.Add( new ActionConstraintItem( action.ActionConstraints[i] ) );
                }

                ExecuteProviders( httpContext, action, items );

                var actionConstraints = ExtractActionConstraints( items );
                var allActionConstraintsCached = true;

                for ( var i = 0; i < items.Count; i++ )
                {
                    var item = items[i];

                    if ( !item.IsReusable )
                    {
                        item.Constraint = null;
                        allActionConstraintsCached = false;
                    }
                }

                if ( allActionConstraintsCached )
                {
                    entry = new CacheEntry( actionConstraints );
                }
                else
                {
                    entry = new CacheEntry( items );
                }

                cache.Entries.TryAdd( action, entry );
                return actionConstraints;
            }

            IReadOnlyList<IActionConstraint>? GetActionConstraintsFromEntry( CacheEntry entry, HttpContext httpContext, ActionDescriptor action )
            {
                if ( entry.ActionConstraints != null )
                {
                    return entry.ActionConstraints;
                }

                var items = new List<ActionConstraintItem>( entry.Items!.Count );

                for ( var i = 0; i < entry.Items.Count; i++ )
                {
                    var item = entry.Items[i];

                    if ( item.IsReusable )
                    {
                        items.Add( item );
                    }
                    else
                    {
                        items.Add( new ActionConstraintItem( item.Metadata ) );
                    }
                }

                ExecuteProviders( httpContext, action, items );

                return ExtractActionConstraints( items );
            }

            void ExecuteProviders( HttpContext httpContext, ActionDescriptor action, List<ActionConstraintItem> items )
            {
                var context = new ActionConstraintProviderContext( httpContext, action, items );

                for ( var i = 0; i < actionConstraintProviders.Length; i++ )
                {
                    actionConstraintProviders[i].OnProvidersExecuting( context );
                }

                for ( var i = actionConstraintProviders.Length - 1; i >= 0; i-- )
                {
                    actionConstraintProviders[i].OnProvidersExecuted( context );
                }
            }

            static IReadOnlyList<IActionConstraint>? ExtractActionConstraints( List<ActionConstraintItem> items )
            {
                var count = 0;

                for ( var i = 0; i < items.Count; i++ )
                {
                    if ( items[i].Constraint != null )
                    {
                        count++;
                    }
                }

                if ( count == 0 )
                {
                    return null;
                }

                var actionConstraints = new IActionConstraint[count];
                var actionConstraintIndex = 0;

                for ( var i = 0; i < items.Count; i++ )
                {
                    var actionConstraint = items[i].Constraint;

                    if ( actionConstraint != null )
                    {
                        actionConstraints[actionConstraintIndex++] = actionConstraint;
                    }
                }

                return actionConstraints;
            }

            internal class InnerCache
            {
                private readonly ActionDescriptorCollection actions;

                public InnerCache( ActionDescriptorCollection actions ) => this.actions = actions;

                public ConcurrentDictionary<ActionDescriptor, CacheEntry> Entries { get; } =
                    new ConcurrentDictionary<ActionDescriptor, CacheEntry>();

                public int Version => actions.Version;
            }

            internal readonly struct CacheEntry
            {
                public CacheEntry( IReadOnlyList<IActionConstraint>? actionConstraints )
                {
                    ActionConstraints = actionConstraints;
                    Items = null;
                }

                public CacheEntry( List<ActionConstraintItem> items )
                {
                    Items = items;
                    ActionConstraints = null;
                }

                public IReadOnlyList<IActionConstraint>? ActionConstraints { get; }

                public List<ActionConstraintItem>? Items { get; }
            }
        }

        sealed partial class StringArrayComparer : IEqualityComparer<string[]>
        {
            public static readonly StringArrayComparer Ordinal = new StringArrayComparer( StringComparer.Ordinal );
            public static readonly StringArrayComparer OrdinalIgnoreCase = new StringArrayComparer( StringComparer.OrdinalIgnoreCase );
            readonly StringComparer valueComparer;

            StringArrayComparer( StringComparer valueComparer ) => this.valueComparer = valueComparer;

            public bool Equals( string[]? x, string[]? y )
            {
                if ( ReferenceEquals( x, y ) )
                {
                    return true;
                }

                if ( x == null || y == null )
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

                var hash = default( HashCode );

                for ( var i = 0; i < obj.Length; i++ )
                {
                    hash.Add( obj[i] ?? string.Empty, valueComparer );
                }

                return hash.ToHashCode();
            }
        }
    }
}