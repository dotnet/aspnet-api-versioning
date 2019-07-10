namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
#if NETSTANDARD2_0
    using Microsoft.AspNetCore.Mvc.Internal;
#else
    using Microsoft.AspNetCore.Mvc.Infrastructure;
#endif
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Linq;
    using static System.Environment;
    using static System.Linq.Enumerable;
    using static System.String;

    /// <summary>
    /// Represents the default API versioning route policy.
    /// </summary>
    [CLSCompliant( false )]
    public class DefaultApiVersionRoutePolicy : IApiVersionRoutePolicy
    {
        readonly IOptions<ApiVersioningOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiVersionRoutePolicy"/> class.
        /// </summary>
        /// <param name="errorResponseProvider">The <see cref="IErrorResponseProvider">provider</see> used to create error responses.</param>
        /// <param name="reportApiVersions">The <see cref="IReportApiVersions">object</see> used to report API versions.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the route policy.</param>
        public DefaultApiVersionRoutePolicy(
            IErrorResponseProvider errorResponseProvider,
            IReportApiVersions reportApiVersions,
            ILoggerFactory loggerFactory,
            IOptions<ApiVersioningOptions> options )
        {
            Arg.NotNull( errorResponseProvider, nameof( errorResponseProvider ) );
            Arg.NotNull( reportApiVersions, nameof( reportApiVersions ) );
            Arg.NotNull( loggerFactory, nameof( loggerFactory ) );
            Arg.NotNull( options, nameof( options ) );

            ErrorResponseProvider = errorResponseProvider;
            ApiVersionReporter = reportApiVersions;
            Logger = loggerFactory.CreateLogger( GetType() );
            this.options = options;
        }

        /// <summary>
        /// Gets the error response provider associated with the route policy.
        /// </summary>
        /// <value>The <see cref="IErrorResponseProvider">provider</see> used to create error responses for the route policy.</value>
        protected IErrorResponseProvider ErrorResponseProvider { get; }

        /// <summary>
        /// Gets the object used to report API versions.
        /// </summary>
        /// <value>The associated <see cref="IReportApiVersions"/> object.</value>
        protected IReportApiVersions ApiVersionReporter { get; }

        /// <summary>
        /// Gets the logger associated with the route policy.
        /// </summary>
        /// <value>The associated <see cref="ILogger">logger</see>.</value>
        protected ILogger Logger { get; }

        /// <summary>
        /// Gets the configuration options associated with the route policy.
        /// </summary>
        /// <value>The associated <see cref="ApiVersioningOptions">service versioning options</see>.</value>
        protected ApiVersioningOptions Options => options.Value;

        /// <summary>
        /// Executes the API versioning route policy.
        /// </summary>
        /// <param name="context">The <see cref="RouteContext">route context</see> to evaluate against.</param>
        /// <param name="selectionResult">The <see cref="ActionSelectionResult">result</see> of action selection.</param>
        /// <returns>The <see cref="ActionDescriptor">action</see> conforming to the policy or <c>null</c>.</returns>
        public virtual ActionDescriptor Evaluate( RouteContext context, ActionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );

            const ActionDescriptor NoMatch = default;

            switch ( selectionResult.MatchingActions.Count )
            {
                case 0:
                    OnUnmatched( context, selectionResult );
                    return NoMatch;
                case 1:
                    return OnSingleMatch( context, selectionResult );
            }

            OnMultipleMatches( context, selectionResult );
            return NoMatch;
        }

        /// <summary>
        /// Occurs when a single action is matched to the route policy.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see>.</param>
        /// <param name="selectionResult">The current <see cref="ActionSelectionResult">action selection result</see>.</param>
        /// <returns>The single, matching <see cref="ActionDescriptor">action</see> conforming to the policy.</returns>
        protected virtual ActionDescriptor OnSingleMatch( RouteContext context, ActionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );
            return selectionResult.BestMatch;
        }

        /// <summary>
        /// Occurs when a no actions are matched by the route policy.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see>.</param>
        /// <param name="selectionResult">The current <see cref="ActionSelectionResult">action selection result</see>.</param>
        protected virtual void OnUnmatched( RouteContext context, ActionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );

            const RequestHandler NotFound = default;
            var candidates = selectionResult.CandidateActions;
            var handler = NotFound;

            if ( candidates.Count > 0 )
            {
                var builder = new ClientErrorBuilder()
                {
                    Options = Options,
                    ApiVersionReporter = ApiVersionReporter,
                    HttpContext = context.HttpContext,
                    Candidates = candidates,
                    Logger = Logger,
                };

                handler = builder.Build();
            }

            context.Handler = handler;
        }

        /// <summary>
        /// Occurs when a multiple actions are matched to the route policy.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see>.</param>
        /// <param name="selectionResult">The current <see cref="ActionSelectionResult">action selection result</see>.</param>
        /// <remarks>The default implementation always throws an <see cref="AmbiguousActionException"/>.</remarks>
        protected virtual void OnMultipleMatches( RouteContext context, ActionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );

            var actionNames = Join( NewLine, selectionResult.MatchingActions.Select( a => a.ExpandSignature() ) );

            Logger.AmbiguousActions( actionNames );

            var message = SR.ActionSelector_AmbiguousActions.FormatDefault( NewLine, actionNames );

            throw new AmbiguousActionException( message );
        }
    }
}