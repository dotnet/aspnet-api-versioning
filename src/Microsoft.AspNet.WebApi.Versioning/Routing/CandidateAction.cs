namespace Microsoft.Web.Http.Routing
{
    using System.Diagnostics;
    using System.Net.Http;
    using System.Web.Http.Controllers;
    using static System.StringComparison;

    [DebuggerDisplay( "{DebuggerToString()}" )]
    sealed class CandidateAction
    {
        const string DebugFormat = "{0}, Order={1}, Prec={2}";

        internal CandidateAction( HttpActionDescriptor actionDescriptor )
            : this( actionDescriptor, default, default ) { }

        internal CandidateAction( HttpActionDescriptor actionDescriptor, int order, decimal precedence )
        {
            ActionDescriptor = actionDescriptor;
            Order = order;
            Precedence = precedence;
        }

        internal string DebuggerToString() => DebugFormat.FormatDefault( ActionDescriptor.ActionName, Order, Precedence );

        public bool MatchName( string actionName ) => string.Equals( ActionDescriptor.ActionName, actionName, OrdinalIgnoreCase );

        public bool MatchVerb( HttpMethod method ) => ActionDescriptor.SupportedHttpMethods.Contains( method );

        public HttpActionDescriptor ActionDescriptor { get; set; }

        public int Order { get; set; }

        public decimal Precedence { get; set; }
    }
}