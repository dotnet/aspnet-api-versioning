// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Dispatcher;

using Asp.Versioning.Routing;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using static Asp.Versioning.ApiVersionMapping;

internal abstract class ControllerSelector
{
    protected ControllerSelector() { }

    public abstract ControllerSelectionResult SelectController( ControllerSelectionContext context );

    protected static ICollection<HttpControllerDescriptor> SelectBestCandidates( IReadOnlyList<CandidateAction> candidates, ApiVersion? apiVersion )
    {
        var bestMatch = default( HttpActionDescriptor );
        var bestMatches = new HashSet<HttpControllerDescriptor>();
        var implicitMatches = new HashSet<HttpControllerDescriptor>();

        for ( var i = 0; i < candidates.Count; i++ )
        {
            var action = candidates[i].ActionDescriptor;
            var metadata = action.GetApiVersionMetadata();

            switch ( metadata.MappingTo( apiVersion ) )
            {
                case Explicit:
                    bestMatch = action;
                    bestMatches.Add( action.ControllerDescriptor );
                    break;
                case Implicit:
                    implicitMatches.Add( action.ControllerDescriptor );
                    break;
            }
        }

        switch ( bestMatches.Count )
        {
            case 0:
                bestMatches.UnionWith( implicitMatches );
                break;
            case 1:
                if ( bestMatch!.GetApiVersionMetadata().IsApiVersionNeutral )
                {
                    bestMatches.UnionWith( implicitMatches );
                }

                break;
        }

        return bestMatches;
    }

    protected static bool TryDisambiguateControllerByAction(
        HttpRequestMessage request,
        IEnumerable<HttpControllerDescriptor> controllers,
        out HttpControllerDescriptor? resolvedController )
    {
        // note: this method should only legitimately be called to disambiguate actions
        // from different controller types that match the request. this can happen as a
        // result of inheritance with a version-neutral action on a base class. there's
        // still a chance the action is actually ambiguous, which is a developer mistake
        var matches = new HashSet<HttpControllerDescriptor>();

        foreach ( var controller in controllers )
        {
            var configuration = controller.Configuration;
            var actionSelector = configuration.Services.GetActionSelector();
            var routeData = EnsureRouteDataSet( configuration, request );
            var context = new HttpControllerContext( configuration, routeData, request )
            {
                ControllerDescriptor = controller,
            };

            try
            {
                if ( actionSelector.SelectAction( context ) != null )
                {
                    matches.Add( controller );
                }
            }
            catch ( InvalidOperationException )
            {
            }
            catch ( HttpResponseException )
            {
            }
        }

        if ( matches.Count == 1 )
        {
            resolvedController = matches.First();
            return true;
        }

        resolvedController = default;
        return false;
    }

    private static IHttpRouteData? EnsureRouteDataSet( HttpConfiguration configuration, HttpRequestMessage request )
    {
        var routeData = request.GetRouteData();

        if ( routeData != null )
        {
            return routeData;
        }

        if ( ( routeData = configuration.Routes.GetRouteData( request ) ) != null )
        {
            request.SetRouteData( routeData );
        }

        return routeData;
    }
}