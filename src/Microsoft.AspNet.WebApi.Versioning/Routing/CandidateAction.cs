namespace Microsoft.Web.Http.Routing
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
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
            Contract.Requires( actionDescriptor != null );
            Contract.Requires( order >= 0 );
            Contract.Requires( precedence >= 0m );

            ActionDescriptor = actionDescriptor;
            Order = order;
            Precedence = precedence;
        }

        [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called by the debugger." )]
        internal string DebuggerToString() => DebugFormat.FormatDefault( ActionDescriptor.ActionName, Order, Precedence );

        public bool MatchName( string actionName ) => string.Equals( ActionDescriptor.ActionName, actionName, OrdinalIgnoreCase );

        public bool MatchVerb( HttpMethod method ) => ActionDescriptor.SupportedHttpMethods.Contains( method );

        public HttpActionDescriptor ActionDescriptor { get; set; }

        public int Order { get; set; }

        public decimal Precedence { get; set; }
    }
}