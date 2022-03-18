// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Dispatcher;

using System.Globalization;
using System.Text;
using System.Web.Http;
using System.Web.Http.Routing;
using static System.Environment;

internal sealed class ConventionRouteControllerSelector : ControllerSelector
{
    private readonly HttpControllerTypeCache controllerTypeCache;

    internal ConventionRouteControllerSelector( HttpControllerTypeCache controllerTypeCache ) => this.controllerTypeCache = controllerTypeCache;

    public override ControllerSelectionResult SelectController( ControllerSelectionContext context )
    {
        var request = context.Request;
        var requestedVersion = context.RequestedVersion;
        var controllerName = context.ControllerName;
        var result = new ControllerSelectionResult()
        {
            RequestedVersion = requestedVersion,
            ControllerName = controllerName,
            HasCandidates = context.HasConventionBasedRoutes,
        };

        if ( !result.HasCandidates )
        {
            return result;
        }

        var bestMatches = SelectBestCandidates( context.ConventionRouteCandidates!, requestedVersion );

        switch ( bestMatches.Count )
        {
            case 0:
                break;
            case 1:
                result.Controller = bestMatches.First();
                result.Controller.SetPossibleCandidates( context.ConventionRouteCandidates!.Select( c => c.ActionDescriptor.ControllerDescriptor ).ToArray() );
                break;
            default:
                if ( TryDisambiguateControllerByAction( request, bestMatches, out var resolvedController ) )
                {
                    result.Controller = resolvedController;
                    break;
                }

                throw CreateAmbiguousControllerException( context.RouteData.Route, controllerName, controllerTypeCache.GetControllerTypes( controllerName ) );
        }

        return result;
    }

    private static Exception CreateAmbiguousControllerException( IHttpRoute route, string? controllerName, ICollection<Type> matchingTypes )
    {
        var builder = new StringBuilder();

        foreach ( var type in matchingTypes )
        {
            builder.AppendLine();
            builder.Append( type.FullName );
        }

        var message = string.Format(
            CultureInfo.CurrentCulture,
            SR.DefaultControllerFactory_ControllerNameAmbiguous_WithRouteTemplate,
            controllerName,
            route.RouteTemplate,
            builder,
            NewLine );

        return new InvalidOperationException( message );
    }
}