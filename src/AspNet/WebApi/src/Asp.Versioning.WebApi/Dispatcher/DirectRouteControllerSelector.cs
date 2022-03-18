// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Dispatcher;

using System.Globalization;
using System.Text;
using System.Web.Http.Controllers;
using static System.Environment;

internal sealed class DirectRouteControllerSelector : ControllerSelector
{
    public override ControllerSelectionResult SelectController( ControllerSelectionContext context )
    {
        var request = context.Request;
        var requestedVersion = context.RequestedVersion;
        var result = new ControllerSelectionResult()
        {
            HasCandidates = context.HasAttributeBasedRoutes,
            RequestedVersion = requestedVersion,
        };

        if ( !result.HasCandidates )
        {
            return result;
        }

        var bestMatches = SelectBestCandidates( context.DirectRouteCandidates!, requestedVersion );

        switch ( bestMatches.Count )
        {
            case 0:
                break;
            case 1:
                result.Controller = bestMatches.First();
                break;
            default:
                if ( TryDisambiguateControllerByAction( request, bestMatches, out var resolvedController ) )
                {
                    result.Controller = resolvedController;
                    break;
                }

                throw CreateAmbiguousControllerException( bestMatches );
        }

        return result;
    }

    private static Exception CreateAmbiguousControllerException( IEnumerable<HttpControllerDescriptor> candidates )
    {
        var types = candidates.Select( c => c.ControllerType ).Distinct();
        var builder = new StringBuilder();

        foreach ( var type in types )
        {
            builder.AppendLine();
            builder.Append( type.FullName );
        }

        var message = string.Format( CultureInfo.CurrentCulture, SR.DirectRoute_AmbiguousController, builder, NewLine );

        return new InvalidOperationException( message );
    }
}