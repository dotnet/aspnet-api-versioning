// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using System.Diagnostics;
using System.Globalization;
using System.Web.Http.Controllers;
using static System.StringComparison;

[DebuggerDisplay( "{DebuggerToString()}" )]
internal sealed class CandidateAction
{
    private const string DebugFormat = "{0}, Order={1}, Prec={2}";

    internal CandidateAction( HttpActionDescriptor actionDescriptor )
        : this( actionDescriptor, default, default ) { }

    internal CandidateAction( HttpActionDescriptor actionDescriptor, int order, decimal precedence )
    {
        ActionDescriptor = actionDescriptor;
        Order = order;
        Precedence = precedence;
    }

    internal string DebuggerToString() => string.Format( CultureInfo.CurrentCulture, DebugFormat, ActionDescriptor.ActionName, Order, Precedence );

    public bool MatchName( string actionName ) => string.Equals( ActionDescriptor.ActionName, actionName, OrdinalIgnoreCase );

    public bool MatchVerb( HttpMethod method ) => ActionDescriptor.SupportedHttpMethods.Contains( method );

    public HttpActionDescriptor ActionDescriptor { get; set; }

    public int Order { get; set; }

    public decimal Precedence { get; set; }
}