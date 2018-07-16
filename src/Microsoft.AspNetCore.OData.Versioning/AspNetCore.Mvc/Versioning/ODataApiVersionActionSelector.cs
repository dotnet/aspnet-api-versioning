namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents the logic for selecting an API-versioned, action method with additional support for OData actions.
    /// </summary>
    [CLSCompliant( false )]
    public class ODataApiVersionActionSelector : ApiVersionActionSelector
    {
        static readonly string ActionKey = ODataRouteConstants.Action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataApiVersionActionSelector"/> class.
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider">The <see cref="IActionDescriptorCollectionProvider "/> used to select candidate routes.</param>
        /// <param name="actionConstraintCache">The <see cref="ActionConstraintCache"/> that providers a set of <see cref="IActionConstraint"/> instances.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the action selector.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="routePolicy">The <see cref="IApiVersionRoutePolicy">route policy</see> applied to candidate matches.</param>
        public ODataApiVersionActionSelector(
            IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
            ActionConstraintCache actionConstraintCache,
            IOptions<ApiVersioningOptions> options,
            ILoggerFactory loggerFactory,
            IApiVersionRoutePolicy routePolicy ) : base( actionDescriptorCollectionProvider, actionConstraintCache, options, loggerFactory, routePolicy ) { }

        /// <summary>
        /// Selects a list of candidate actions from the specified route context.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see> to evaluate.</param>
        /// <returns>A <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="ActionDescriptor">actions</see>.</returns>
        public override IReadOnlyList<ActionDescriptor> SelectCandidates( RouteContext context )
        {
            Arg.NotNull( context, nameof( context ) );

            var odataPath = context.HttpContext.ODataFeature().Path;
            var routeValues = context.RouteData.Values;
            var notODataCandidate = odataPath == null || routeValues.ContainsKey( ActionKey );

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

            var possibleCandidates = new List<ActionCandidate>();

            foreach ( var convention in routingConventions )
            {
                var actionDescriptor = convention.SelectAction( context );

                if ( actionDescriptor != null )
                {
                    possibleCandidates.AddRange( actionDescriptor.Select( a => new ActionCandidate( a ) ) );
                }
            }

            if ( possibleCandidates.Count == 0 )
            {
                return base.SelectCandidates( context );
            }

            var availableKeys = new HashSet<string>( routeValues.Keys, StringComparer.OrdinalIgnoreCase );
            var bestCandidates = possibleCandidates.Where( c => c.FilteredParameters.All( availableKeys.Contains ) ).Select( c => c.Action ).ToArray();

            return bestCandidates.Length == 0 ? base.SelectCandidates( context ) : bestCandidates;
        }

        /// <summary>
        /// Selects the best action given the provided route context and list of candidate actions.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see> to evaluate.</param>
        /// <param name="candidates">The <see cref="IReadOnlyList{T}">read-only list</see> of candidate <see cref="ActionDescriptor">actions</see> to select from.</param>
        /// <returns>The best candidate <see cref="ActionDescriptor">action</see> or <c>null</c> if no candidate matches.</returns>
        public override ActionDescriptor SelectBestCandidate( RouteContext context, IReadOnlyList<ActionDescriptor> candidates )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( candidates, nameof( candidates ) );

            var httpContext = context.HttpContext;
            var odataPath = httpContext.ODataFeature().Path;
            var routeValues = context.RouteData.Values;
            var odataRouteCandidate = odataPath != null && routeValues.ContainsKey( ActionKey );

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
            var finalMatch = bestActions.Select( action => new ActionCandidate( action ) )
                                        .OrderByDescending( candidate => candidate.FilteredParameters.Count )
                                        .ThenByDescending( candidate => candidate.TotalParameterCount )
                                        .FirstOrDefault()?.Action;
            IReadOnlyList<ActionDescriptor> finalMatches = finalMatch == null ? Array.Empty<ActionDescriptor>() : new[] { finalMatch };
            var feature = httpContext.Features.Get<IApiVersioningFeature>();
            var selectionResult = feature.SelectionResult;

            feature.RequestedApiVersion = selectionContext.RequestedVersion;
            selectionResult.AddCandidates( candidates );

            if ( finalMatches.Count == 0 )
            {
                return null;
            }

            selectionResult.AddMatches( finalMatches );

            var bestCandidate = RoutePolicy.Evaluate( context, selectionResult );

            if ( bestCandidate is ControllerActionDescriptor controllerAction )
            {
                routeValues[ActionKey] = controllerAction.ActionName;
            }

            return bestCandidate;
        }

        static bool IsNotSatisfiedByODataModelBinder( ParameterDescriptor parameter )
        {
            Contract.Requires( parameter != null );

            var type = parameter.ParameterType;
            return type != typeof( ODataParameterHelper ) && !type.IsODataQueryOptions() && !type.IsDelta();
        }

        [DebuggerDisplay( "{Action.DisplayName,nq}" )]
        sealed class ActionCandidate
        {
            internal ActionCandidate( ActionDescriptor action )
            {
                Contract.Requires( action != null );

                Action = action;
                FilteredParameters = action.Parameters.Where( IsNotSatisfiedByODataModelBinder ).Select( p => p.Name ).ToArray();
            }

            internal ActionDescriptor Action { get; }

            internal int TotalParameterCount => Action.Parameters.Count;

            internal IReadOnlyList<string> FilteredParameters { get; }
        }
    }
}