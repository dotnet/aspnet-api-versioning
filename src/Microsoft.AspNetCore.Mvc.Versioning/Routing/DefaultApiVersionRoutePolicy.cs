namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading.Tasks;
    using Versioning;
    using static ApiVersion;
    using static System.Environment;
    using static System.Linq.Enumerable;
    using static System.String;
    using static Versioning.ErrorCodes;

    /// <summary>
    /// Represents the default API versioning route policy.
    /// </summary>
    [CLSCompliant( false )]
    public class DefaultApiVersionRoutePolicy : IApiVersionRoutePolicy
    {
        static readonly Task CompletedTask = Task.FromResult( default( object ) );
        readonly IOptions<ApiVersioningOptions> options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiVersionRoutePolicy"/> class.
        /// </summary>
        /// <param name="actionInvokerFactory">The underlying <see cref="IActionInvokerFactory">action invoker factory</see>.</param>
        /// <param name="errorResponseProvider">The <see cref="IErrorResponseProvider">provider</see> used to create error responses.</param>
        /// <param name="reportApiVersions">The <see cref="IReportApiVersions">object</see> used to report API versions.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the route policy.</param>
        public DefaultApiVersionRoutePolicy(
            IActionInvokerFactory actionInvokerFactory,
            IErrorResponseProvider errorResponseProvider,
            IReportApiVersions reportApiVersions,
            ILoggerFactory loggerFactory,
            IOptions<ApiVersioningOptions> options = null )
            : this( actionInvokerFactory, errorResponseProvider, reportApiVersions, loggerFactory, null, options ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiVersionRoutePolicy"/> class.
        /// </summary>
        /// <param name="actionInvokerFactory">The underlying <see cref="IActionInvokerFactory">action invoker factory</see>.</param>
        /// <param name="errorResponseProvider">The <see cref="IErrorResponseProvider">provider</see> used to create error responses.</param>
        /// <param name="reportApiVersions">The <see cref="IReportApiVersions">object</see> used to report API versions.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="actionContextAccessor">The associated <see cref="IActionContextAccessor">action context accessor</see>.</param>
        /// <param name="options">The <see cref="ApiVersioningOptions">options</see> associated with the route policy.</param>
        public DefaultApiVersionRoutePolicy(
            IActionInvokerFactory actionInvokerFactory,
            IErrorResponseProvider errorResponseProvider,
            IReportApiVersions reportApiVersions,
            ILoggerFactory loggerFactory,
            IActionContextAccessor actionContextAccessor,
            IOptions<ApiVersioningOptions> options = null )
        {
            Arg.NotNull( actionInvokerFactory, nameof( actionInvokerFactory ) );
            Arg.NotNull( errorResponseProvider, nameof( errorResponseProvider ) );
            Arg.NotNull( reportApiVersions, nameof( reportApiVersions ) );
            Arg.NotNull( loggerFactory, nameof( loggerFactory ) );

            ErrorResponseProvider = errorResponseProvider;
            ActionInvokerFactory = actionInvokerFactory;
            ApiVersionReporter = reportApiVersions;
            Logger = loggerFactory.CreateLogger( GetType() );
            ActionContextAccessor = actionContextAccessor;
            this.options = options ?? new OptionsWrapper<ApiVersioningOptions>( new ApiVersioningOptions() );
        }

        /// <summary>
        /// Gets the action invoker factory associated with the route policy.
        /// </summary>
        /// <value>The associated <see cref="IActionInvokerFactory"/>.</value>
        protected IActionInvokerFactory ActionInvokerFactory { get; }

        /// <summary>
        /// Gets the action context accessor associated with the route policy, if any.
        /// </summary>
        /// <value>The associated <see cref="IActionContextAccessor"/> or <c>null</c>.</value>
        protected IActionContextAccessor ActionContextAccessor { get; }

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
        /// Gets the virtual path given the specified context.
        /// </summary>
        /// <param name="context">The <see cref="VirtualPathContext">virtual path context</see> used to retrieve the path data.</param>
        /// <returns>The <see cref="VirtualPathData">virtual path data</see>. The default implementation always returns <c>null</c>.</returns>
        public virtual VirtualPathData GetVirtualPath( VirtualPathContext context ) => null;

        /// <summary>
        /// Executes the API versioning route policy.
        /// </summary>
        /// <param name="context">The <see cref="RouteContext">route context</see> to evaluate against.</param>
        /// <returns>A <see cref="Task">task</see> representing the asynchonrous operation.</returns>
        public virtual Task RouteAsync( RouteContext context )
        {
            var selectionResult = context.HttpContext.ApiVersionProperties().SelectionResult;
            var match = selectionResult.BestMatch;

            if ( match == null )
            {
                var hasAnyMatches = selectionResult.MatchingActions.SelectMany( i => i.Value ).Any();

                if ( hasAnyMatches )
                {
                    OnMultipleMatches( context, selectionResult );
                }
                else
                {
                    OnUnmatched( context, selectionResult );
                }
            }
            else
            {
                OnSingleMatch( context, selectionResult, selectionResult.BestMatch );
            }

            return CompletedTask;
        }

        /// <summary>
        /// Occurs when a single action is matched to the route policy.
        /// </summary>
        /// <param name="context">The current <see cref="RouteContext">route context</see>.</param>
        /// <param name="selectionResult">The current <see cref="ActionSelectionResult">action selection result</see>.</param>
        /// <param name="match">The <see cref="ActionDescriptorMatch">matched</see> action.</param>
        protected virtual void OnSingleMatch( RouteContext context, ActionSelectionResult selectionResult, ActionDescriptorMatch match )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );
            Arg.NotNull( match, nameof( match ) );

            var handler = new DefaultActionHandler( ActionInvokerFactory, ActionContextAccessor, selectionResult, match );
            var candidates = selectionResult.CandidateActions.SelectMany( kvp => kvp.Value );

            match.Action.AggregateAllVersions( candidates );
            context.RouteData = match.RouteData;
            context.Handler = handler.Invoke;
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

            context.Handler = ClientError( context.HttpContext, selectionResult );
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

            var matchingActions = selectionResult.MatchingActions.OrderBy( kvp => kvp.Key ).SelectMany( kvp => kvp.Value ).Distinct();
            var actionNames = Join( NewLine, matchingActions.Select( match => match.Action.DisplayName ) );

            Logger.AmbiguousActions( actionNames );

            var message = SR.ActionSelector_AmbiguousActions.FormatDefault( NewLine, actionNames );

            throw new AmbiguousActionException( message );
        }

        RequestHandler ClientError( HttpContext httpContext, ActionSelectionResult selectionResult )
        {
            Contract.Requires( httpContext != null );
            Contract.Requires( selectionResult != null );

            const RequestHandler NotFound = default( RequestHandler );
            var candidates = selectionResult.CandidateActions.OrderBy( kvp => kvp.Key ).SelectMany( kvp => kvp.Value ).Distinct().ToArray();

            if ( candidates.Length == 0 )
            {
                return NotFound;
            }

            var properties = httpContext.ApiVersionProperties();
            var method = httpContext.Request.Method;
            var requestUrl = new Lazy<string>( httpContext.Request.GetDisplayUrl );
            var requestedVersion = properties.RawApiVersion;
            var parsedVersion = properties.ApiVersion;
            var actionNames = new Lazy<string>( () => Join( NewLine, candidates.Select( a => a.DisplayName ) ) );
            var allowedMethods = new Lazy<HashSet<string>>( () => AllowedMethodsFromCandidates( candidates ) );
            var apiVersions = new Lazy<ApiVersionModel>( selectionResult.CandidateActions.SelectMany( l => l.Value.Select( a => a.GetProperty<ApiVersionModel>() ).Where( m => m != null ) ).Aggregate );
            var handlerContext = new RequestHandlerContext( ErrorResponseProvider, ApiVersionReporter, apiVersions );

            if ( parsedVersion == null )
            {
                if ( IsNullOrEmpty( requestedVersion ) )
                {
                    if ( Options.AssumeDefaultVersionWhenUnspecified || candidates.Any( c => c.IsApiVersionNeutral() ) )
                    {
                        return VersionNeutralUnmatched( handlerContext, requestUrl.Value, method, allowedMethods.Value, actionNames.Value );
                    }

                    return UnspecifiedApiVersion( handlerContext, actionNames.Value );
                }
                else if ( !TryParse( requestedVersion, out parsedVersion ) )
                {
                    return MalformedApiVersion( handlerContext, requestUrl.Value, requestedVersion );
                }
            }
            else if ( IsNullOrEmpty( requestedVersion ) )
            {
                return VersionNeutralUnmatched( handlerContext, requestUrl.Value, method, allowedMethods.Value, actionNames.Value );
            }
            else
            {
                requestedVersion = parsedVersion.ToString();
            }

            return Unmatched( handlerContext, requestUrl.Value, method, allowedMethods.Value, actionNames.Value, parsedVersion, requestedVersion );
        }

        static HashSet<string> AllowedMethodsFromCandidates( IEnumerable<ActionDescriptor> candidates )
        {
            Contract.Requires( candidates != null );
            Contract.Ensures( Contract.Result<HashSet<string>>() != null );

            var httpMethods = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

            foreach ( var candidate in candidates )
            {
                if ( candidate.ActionConstraints == null )
                {
                    continue;
                }

                foreach ( var constraint in candidate.ActionConstraints.OfType<HttpMethodActionConstraint>() )
                {
                    httpMethods.AddRange( constraint.HttpMethods );
                }
            }

            return httpMethods;
        }

        RequestHandler VersionNeutralUnmatched( RequestHandlerContext context, string requestUrl, string method, IReadOnlyCollection<string> allowedMethods, string actionNames )
        {
            Contract.Requires( context != null );
            Contract.Requires( !IsNullOrEmpty( requestUrl ) );
            Contract.Requires( !IsNullOrEmpty( method ) );
            Contract.Requires( allowedMethods != null );

            Logger.ApiVersionUnspecified( actionNames );
            context.Code = UnsupportedApiVersion;

            if ( allowedMethods.Contains( method ) )
            {
                context.Message = SR.VersionNeutralResourceNotSupported.FormatDefault( requestUrl );
                return new BadRequestHandler( context );
            }

            context.Message = SR.VersionNeutralMethodNotSupported.FormatDefault( requestUrl, method );
            context.AllowedMethods = allowedMethods.ToArray();

            return new MethodNotAllowedHandler( context );
        }

        RequestHandler UnspecifiedApiVersion( RequestHandlerContext context, string actionNames )
        {
            Contract.Requires( context != null );

            Logger.ApiVersionUnspecified( actionNames );

            context.Code = ApiVersionUnspecified;
            context.Message = SR.ApiVersionUnspecified;

            return new BadRequestHandler( context );
        }

        RequestHandler MalformedApiVersion( RequestHandlerContext context, string requestUrl, string requestedVersion )
        {
            Contract.Requires( context != null );
            Contract.Requires( !IsNullOrEmpty( requestUrl ) );
            Contract.Requires( !IsNullOrEmpty( requestedVersion ) );

            Logger.ApiVersionInvalid( requestedVersion );

            context.Code = InvalidApiVersion;
            context.Message = SR.VersionedResourceNotSupported.FormatDefault( requestUrl, requestedVersion );

            return new BadRequestHandler( context );
        }

        RequestHandler Unmatched(
            RequestHandlerContext context,
            string requestUrl,
            string method,
            IReadOnlyCollection<string> allowedMethods,
            string actionNames,
            ApiVersion parsedVersion,
            string requestedVersion )
        {
            Contract.Requires( context != null );
            Contract.Requires( !IsNullOrEmpty( requestUrl ) );
            Contract.Requires( !IsNullOrEmpty( method ) );
            Contract.Requires( allowedMethods != null );
            Contract.Requires( parsedVersion != null );
            Contract.Requires( !IsNullOrEmpty( requestedVersion ) );

            Logger.ApiVersionUnmatched( parsedVersion, actionNames );

            context.Code = UnsupportedApiVersion;

            if ( allowedMethods.Contains( method ) )
            {
                context.Message = SR.VersionedResourceNotSupported.FormatDefault( requestUrl, requestedVersion );
                return new BadRequestHandler( context );
            }

            context.Message = SR.VersionedMethodNotSupported.FormatDefault( requestUrl, requestedVersion, method );
            context.AllowedMethods = allowedMethods.ToArray();

            return new MethodNotAllowedHandler( context );
        }

        sealed class DefaultActionHandler
        {
            readonly IActionContextAccessor actionContextAccessor;
            readonly IActionInvokerFactory actionInvokerFactory;
            readonly ActionSelectionResult selectionResult;
            readonly ActionDescriptorMatch match;

            internal DefaultActionHandler(
                IActionInvokerFactory actionInvokerFactory,
                IActionContextAccessor actionContextAccessor,
                ActionSelectionResult selectionResult,
                ActionDescriptorMatch match )
            {
                this.actionContextAccessor = actionContextAccessor;
                this.actionInvokerFactory = actionInvokerFactory;
                this.selectionResult = selectionResult;
                this.match = match;
            }

            internal Task Invoke( HttpContext context )
            {
                Contract.Requires( context != null );

                var actionContext = new ActionContext( context, match.RouteData, match.Action );

                if ( actionContextAccessor != null )
                {
                    actionContextAccessor.ActionContext = actionContext;
                }

                var invoker = actionInvokerFactory.CreateInvoker( actionContext );

                if ( invoker == null )
                {
                    throw new InvalidOperationException( SR.CouldNotCreateInvoker.FormatDefault( match.Action.DisplayName ) );
                }

                return invoker.InvokeAsync();
            }
        }
    }
}