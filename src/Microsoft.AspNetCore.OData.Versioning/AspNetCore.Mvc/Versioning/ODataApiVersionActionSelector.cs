namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents the logic for selecting an API-versioned, action method with additional support for OData actions.
    /// </summary>
    [CLSCompliant( false )]
    public class ODataApiVersionActionSelector : ApiVersionActionSelector
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiVersionActionSelector"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider "/> used to select candidate routes.</param>
        /// <param name="actionConstraintProviders">The <see cref="IEnumerable{T}">sequence</see> of <see cref="IActionConstraintProvider">action constraint providers</see>.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="routePolicy">The <see cref="IApiVersionRoutePolicy">route policy</see> applied to candidate matches.</param>
        public ODataApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            IEnumerable<IActionConstraintProvider> actionConstraintProviders,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy ) : base( actionDescriptorCollectionProvider, actionConstraintProviders, options, loggerFactory, routePolicy ) { }

        /// <summary>
        /// Selects a list of candidate actions from the specified route context.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="ActionDescriptor">actions</see>.</returns>
        public override IReadOnlyList<ActionDescriptor> SelectCandidates( RouteContext context )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            var odataPath = context.HttpContext.ODataFeature().Path;
            var routeValues = context.RouteData.Values;
            var notODataCandidate = odataPath == null || routeValues.ContainsKey( ODataRouteConstants.Action );

            if ( notODataCandidate )
            {
                return base.SelectCandidates( context );
            }

            var routeData = context.RouteData;
            var routingConventions = context.HttpContext.Request.GetRoutingConventions();

            if ( routingConventions == null )
            {
                return base.SelectCandidates( context );
            }

            var visited = new HashSet<ActionDescriptor>();
            var possibleCandidates = new List<ActionCandidate>();

            foreach ( var convention in routingConventions )
            {
                var actions = convention.SelectAction( context );

                if ( actions == null )
                {
                    continue;
                }

                foreach ( var action in actions )
                {
                    if ( visited.Add( action ) )
                    {
                        possibleCandidates.Add( new ActionCandidate( action ) );
                    }
                }
            }

            if ( possibleCandidates.Count == 0 )
            {
                return base.SelectCandidates( context );
            }

            var availableKeys = new HashSet<string>( routeValues.Keys, StringComparer.OrdinalIgnoreCase );
            var bestCandidates = new List<ActionDescriptor>( possibleCandidates.Count );

            availableKeys.Remove( ODataRouteConstants.ODataPath );

            for ( var i = 0; i < possibleCandidates.Count; i++ )
            {
                var possibleCandidate = possibleCandidates[i];

                if ( availableKeys.Count == 0 )
                {
                    if ( possibleCandidate.FilteredParameters.Count == 0 )
                    {
                        bestCandidates.Add( possibleCandidate.Action );
                    }
                }
                else if ( possibleCandidate.FilteredParameters.All( availableKeys.Contains ) )
                {
                    bestCandidates.Add( possibleCandidate.Action );
                }
            }

            return bestCandidates;
        }

        /// <summary>
        /// Selects the best action given the provided route context and list of candidate actions.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see> to evaluate.</param>
        /// <param name="candidates">The <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="ActionDescriptor">actions</see> to select from.</param>
        /// <returns>The best candidate <see cref="ActionDescriptor">action</see> or <c>null</c> if no candidate matches.</returns>
        public override ActionDescriptor? SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates )
        {
            if ( context == null )
            {
                throw new ArgumentNullException( nameof( context ) );
            }

            if ( candidates == null )
            {
                throw new ArgumentNullException( nameof( candidates ) );
            }

            var httpContext = context.HttpContext;
            var odataRouteCandidate = httpContext.ODataFeature().Path != null;

            if ( !odataRouteCandidate )
            {
                return base.SelectBestCandidate( context, candidates );
            }

            if ( IsRequestedApiVersionAmbiguous( context, out var apiVersion ) )
            {
                return null;
            }

            var matches = EvaluateActionConstraints( context, candidates );
            var selectionContext = new ActionSelectionContext( httpContext, matches, apiVersion );
            var bestActions = SelectBestActions( selectionContext );
            var finalMatches = bestActions.Select( action => new ActionCandidate( action ) )
                                          .OrderByDescending( candidate => candidate.FilteredParameters.Count )
                                          .ThenByDescending( candidate => candidate.TotalParameterCount )
                                          .Take( 1 )
                                          .Select( candidate => candidate.Action )
                                          .ToArray();
            var feature = httpContext.Features.Get<IApiVersioningFeature>();
            var selectionResult = feature.SelectionResult;

            feature.RequestedApiVersion = selectionContext.RequestedVersion;
            selectionResult.AddCandidates( candidates );

            if ( finalMatches.Length == 0 )
            {
                return null;
            }

            selectionResult.AddMatches( finalMatches );

            return RoutePolicy.Evaluate( context, selectionResult );
        }
    }
}