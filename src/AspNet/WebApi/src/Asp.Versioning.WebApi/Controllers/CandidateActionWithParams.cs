// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Controllers;

using Asp.Versioning.Routing;
using System.Diagnostics;
using System.Text;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

[DebuggerDisplay( "{DebuggerToString()}" )]
internal sealed class CandidateActionWithParams
{
    internal CandidateActionWithParams(
        CandidateAction candidateAction,
        ISet<string> parameters,
        IHttpRouteData routeDataSource )
    {
        CandidateAction = candidateAction;
        CombinedParameterNames = parameters;
        RouteDataSource = routeDataSource;
    }

    internal CandidateAction CandidateAction { get; }

    internal ISet<string> CombinedParameterNames { get; }

    internal IHttpRouteData RouteDataSource { get; }

    internal HttpActionDescriptor ActionDescriptor => CandidateAction.ActionDescriptor;

    private string DebuggerToString()
    {
        if ( CombinedParameterNames.Count < 1 )
        {
            return CandidateAction.DebuggerToString();
        }

        var text = new StringBuilder();

        text.Append( CandidateAction.DebuggerToString() ).Append( ", Params =" );

        foreach ( var param in CombinedParameterNames )
        {
            text.Append( ' ' ).Append( param );
        }

        return text.ToString();
    }
}