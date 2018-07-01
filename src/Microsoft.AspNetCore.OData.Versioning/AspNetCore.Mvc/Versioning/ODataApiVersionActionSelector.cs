namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ActionConstraints;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
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
            var newOrExistingODataRouteCandidate = odataPath == null || routeValues.ContainsKey( ActionKey );

            if ( newOrExistingODataRouteCandidate )
            {
                return base.SelectCandidates( context );
            }

            var routeData = context.RouteData;
            var routingConventions = context.HttpContext.Request.GetRoutingConventions();

            if ( routingConventions == null )
            {
                return base.SelectCandidates( context );
            }

            foreach ( var convention in routingConventions )
            {
                var actionDescriptor = convention.SelectAction( context );

                if ( actionDescriptor?.Any() == true )
                {
                    routeData.Values[ActionKey] = actionDescriptor.First().ActionName;
                    return actionDescriptor.ToArray();
                }
            }

            return base.SelectCandidates( context );
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

            var bestCandidates = new ConcurrentDictionary<int, List<ActionDescriptor>>();

            foreach ( var candidate in candidates )
            {
                var parameters = candidate.Parameters.Where( p => p.BindingInfo != null ).ToArray();

                if ( parameters.Length == 0 || parameters.Any( p => routeValues.ContainsKey( p.Name ) ) )
                {
                    bestCandidates.GetOrAdd( candidate.Parameters.Count, key => new List<ActionDescriptor>() ).Add( candidate );
                }
            }

            var matches = candidates;

            if ( bestCandidates.Count > 0 )
            {
                var key = bestCandidates.Keys.Max();
                matches = bestCandidates[key];
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
    }
}