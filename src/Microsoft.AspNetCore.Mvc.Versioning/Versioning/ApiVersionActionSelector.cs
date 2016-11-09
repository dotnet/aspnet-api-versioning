namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using ActionConstraints;
    using AspNetCore.Routing;
    using Extensions.Logging;
    using Extensions.Options;
    using Http;
    using Http.Extensions;
    using Infrastructure;
    using Internal;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using static ApiVersion;
    using static System.Environment;
    using static System.String;

    /// <summary>
    /// Represents the logic for selecting an API-versioned, action method.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersionActionSelector : IActionSelector
    {
        private static readonly IReadOnlyList<ActionDescriptor> NoMatches = new ActionDescriptor[0];
        private readonly IActionSelectorDecisionTreeProvider decisionTreeProvider;
        private readonly ActionConstraintCache actionConstraintCache;
        private readonly IOptions<ApiVersioningOptions> options;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionActionSelector"/> class.
        /// </summary>
        /// <param name="decisionTreeProvider">The <see cref="IActionSelectorDecisionTreeProvider"/> used to select candidate routes.</param>
        /// <param name="actionConstraintCache">The <see cref="ActionConstraintCache"/> that providers a set of <see cref="IActionConstraint"/> instances.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public ApiVersionActionSelector(
            IActionSelectorDecisionTreeProvider decisionTreeProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory )
        {
            this.decisionTreeProvider = decisionTreeProvider;
            this.actionConstraintCache = actionConstraintCache;
            this.options = options;
            logger = loggerFactory.CreateLogger<ApiVersionActionSelector>();
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
        /// Selects a list of candidate actions from the specified route context.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="ActionDescriptor">actions</see>.</returns>
        public virtual IReadOnlyList<ActionDescriptor> SelectCandidates( RouteContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            var tree = decisionTreeProvider.DecisionTree;
            return tree.Select( context.RouteData.Values );
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
            var apiVersion = default( ApiVersion );
            var badRequest = default( BadRequestHandler );

            if ( ( badRequest = VerifyRequestedApiVersionIsNotAmbiguous( httpContext, out apiVersion ) ) != null )
            {
                context.Handler = badRequest;
                return null;
            }

            var matches = EvaluateActionConstraints( context, candidates );
            var selectionContext = new ActionSelectionContext( httpContext, matches, apiVersion );
            var finalMatches = SelectBestActions( selectionContext );

            if ( finalMatches == null || finalMatches.Count == 0 )
            {
                if ( ( badRequest = IsValidRequest( selectionContext ) ) != null )
                {
                    context.Handler = badRequest;
                }

                return null;
            }
            else if ( finalMatches.Count == 1 )
            {
                var selectedAction = finalMatches[0];
                selectedAction.AggregateAllVersions( selectionContext );
                httpContext.SetRequestedApiVersion( selectionContext.RequestedVersion );
                return selectedAction;
            }
            else
            {
                var actionNames = Join( NewLine, finalMatches.Select( a => a.DisplayName ) );

                logger.AmbiguousActions( actionNames );

                var message = SR.ActionSelector_AmbiguousActions.FormatDefault( NewLine, actionNames );

                throw new AmbiguousActionException( message );
            }
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

        private BadRequestHandler VerifyRequestedApiVersionIsNotAmbiguous( HttpContext httpContext, out ApiVersion apiVersion )
        {
            Contract.Requires( httpContext != null );

            try
            {
                apiVersion = httpContext.GetRequestedApiVersion();
            }
            catch ( AmbiguousApiVersionException ex )
            {
                logger.LogInformation( ex.Message );
                apiVersion = default( ApiVersion );
                return new BadRequestHandler( Options, "AmbiguousApiVersion", ex.Message );
            }

            return null;
        }

        private BadRequestHandler IsValidRequest( ActionSelectionContext context )
        {
            Contract.Requires( context != null );

            if ( !context.MatchingActions.Any() )
            {
                return null;
            }

            var code = default( string );
            var requestedVersion = default( string );
            var parsedVersion = context.RequestedVersion;
            var actionNames = new Lazy<string>( () => Join( NewLine, context.MatchingActions.Select( a => a.DisplayName ) ) );

            if ( parsedVersion == null )
            {
                requestedVersion = context.HttpContext.GetRawRequestedApiVersion();

                if ( IsNullOrEmpty( requestedVersion ) )
                {
                    logger.ApiVersionUnspecified( actionNames.Value );
                    return null;
                }
                else if ( TryParse( requestedVersion, out parsedVersion ) )
                {
                    code = "UnsupportedApiVersion";
                    logger.ApiVersionUnmatched( parsedVersion, actionNames.Value );
                }
                else
                {
                    code = "InvalidApiVersion";
                    logger.ApiVersionInvalid( requestedVersion );
                }
            }
            else
            {
                requestedVersion = parsedVersion.ToString();
                code = "UnsupportedApiVersion";
                logger.ApiVersionUnmatched( parsedVersion, actionNames.Value );
            }

            var message = SR.VersionedResourceNotSupported.FormatDefault( context.HttpContext.Request.GetDisplayUrl(), requestedVersion );
            return new BadRequestHandler( Options, code, message );
        }

        private static IEnumerable<ActionDescriptor> MatchVersionNeutralActions( ActionSelectionContext context ) =>
            from action in context.MatchingActions
            let model = action.GetProperty<ApiVersionModel>()
            where model?.IsApiVersionNeutral ?? false
            select action;

        private static bool ActionIsSatisfiedBy( ActionDescriptor action, ApiVersionModel model, ApiVersion version, ICollection<ActionDescriptor> implicitMatches )
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

        private IReadOnlyList<ActionDescriptor> EvaluateActionConstraints( RouteContext context, IReadOnlyList<ActionDescriptor> actions )
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

        private IReadOnlyList<ActionSelectorCandidate> EvaluateActionConstraintsCore( RouteContext context, IReadOnlyList<ActionSelectorCandidate> candidates, int? startingOrder )
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
            var constraintContext = new ActionConstraintContext();

            constraintContext.Candidates = candidates;
            constraintContext.RouteContext = context;

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
                            logger.ConstraintMismatch( candidate.Action.DisplayName, candidate.Action.Id, constraint );
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
    }
}