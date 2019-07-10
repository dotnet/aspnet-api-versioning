namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
#if NETSTANDARD2_0
    using Microsoft.AspNetCore.Mvc.Internal;
#endif
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static ApiVersionMapping;
    using static ErrorCodes;
    using static System.Globalization.CultureInfo;

    /// <summary>
    /// Represents the logic for selecting an API-versioned, action method.
    /// </summary>
    [CLSCompliant( false )]
    public partial class ApiVersionActionSelector : IActionSelector
    {
        static readonly IReadOnlyList<ActionDescriptor> NoMatches = Array.Empty<ActionDescriptor>();
        readonly IActionDescriptorCollectionProvider actionDescriptorCollectionProvider;
        readonly IOptions<ApiVersioningOptions> options;
        readonly ActionConstraintCache actionConstraintCache;

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

        sealed partial class StringArrayComparer : IEqualityComparer<string[]>
        {
            public static readonly StringArrayComparer Ordinal = new StringArrayComparer( StringComparer.Ordinal );
            public static readonly StringArrayComparer OrdinalIgnoreCase = new StringArrayComparer( StringComparer.OrdinalIgnoreCase );
            private readonly StringComparer valueComparer;

            private StringArrayComparer( StringComparer valueComparer )
            {
                this.valueComparer = valueComparer;
            }

            public bool Equals( string[] x, string[] y )
            {
                if ( object.ReferenceEquals( x, y ) )
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
        }
    }
}