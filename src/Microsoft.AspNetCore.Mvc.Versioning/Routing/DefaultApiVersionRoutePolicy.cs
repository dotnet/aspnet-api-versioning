namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Extensions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Logging;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiVersionRoutePolicy"/> class.
        /// </summary>
        /// <param name="actionInvokerFactory">The underlying <see cref="IActionInvokerFactory">action invoker factory</see>.</param>
        /// <param name="errorResponseProvider">The <see cref="IErrorResponseProvider">provider</see> used to create error responses.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        public DefaultApiVersionRoutePolicy( IActionInvokerFactory actionInvokerFactory, IErrorResponseProvider errorResponseProvider, ILoggerFactory loggerFactory )
            : this( actionInvokerFactory, errorResponseProvider, loggerFactory, null ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultApiVersionRoutePolicy"/> class.
        /// </summary>
        /// <param name="actionInvokerFactory">The underlying <see cref="IActionInvokerFactory">action invoker factory</see>.</param>
        /// <param name="errorResponseProvider">The <see cref="IErrorResponseProvider">provider</see> used to create error responses.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="actionContextAccessor">The associated <see cref="IActionContextAccessor">action context accessor</see>.</param>
        public DefaultApiVersionRoutePolicy(
            IActionInvokerFactory actionInvokerFactory,
            IErrorResponseProvider errorResponseProvider,
            ILoggerFactory loggerFactory,
            IActionContextAccessor actionContextAccessor )
        {
            Arg.NotNull( actionInvokerFactory, nameof( actionInvokerFactory ) );
            Arg.NotNull( errorResponseProvider, nameof( errorResponseProvider ) );
            Arg.NotNull( loggerFactory, nameof( loggerFactory ) );

            ErrorResponseProvider = errorResponseProvider;
            ActionInvokerFactory = actionInvokerFactory;
            Logger = loggerFactory.CreateLogger( GetType() );
            ActionContextAccessor = actionContextAccessor;
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
        /// Gets the logger associated with the route policy.
        /// </summary>
        /// <value>The associated <see cref="ILogger">logger</see>.</value>
        protected ILogger Logger { get; }

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

            context.Handler = ClientError( context, selectionResult );
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

        RequestHandler ClientError( RouteContext context, ActionSelectionResult selectionResult )
        {
            Contract.Requires( context != null );
            Contract.Requires( selectionResult != null );

            const RequestHandler NotFound = default( RequestHandler );
            var candidates = selectionResult.CandidateActions.OrderBy( kvp => kvp.Key ).SelectMany( kvp => kvp.Value ).Distinct().ToArray();

            if ( candidates.Length == 0 )
            {
                return NotFound;
            }

            var httpContext = context.HttpContext;
            var properties = httpContext.ApiVersionProperties();
            var code = default( string );
            var message = default( string );
            var requestedVersion = default( string );
            var parsedVersion = properties.ApiVersion;
            var actionNames = new Lazy<string>( () => Join( NewLine, candidates.Select( a => a.DisplayName ) ) );
            var allowedMethods = new Lazy<HashSet<string>>( () => AllowedMethodsFromCandidates( candidates ) );
            var newRequestHandler = default( Func<IErrorResponseProvider, string, string, RequestHandler> );

            if ( parsedVersion == null )
            {
                var versionNeutral = new Lazy<bool>( () => candidates.Any( c => c.IsApiVersionNeutral() ) );

                requestedVersion = properties.RawApiVersion;

                if ( IsNullOrEmpty( requestedVersion ) && !versionNeutral.Value )
                {
                    code = ApiVersionUnspecified;
                    Logger.ApiVersionUnspecified( actionNames.Value );
                    return new BadRequestHandler( ErrorResponseProvider, code, SR.ApiVersionUnspecified );
                }
                else if ( TryParse( requestedVersion, out parsedVersion ) )
                {
                    code = UnsupportedApiVersion;
                    Logger.ApiVersionUnmatched( parsedVersion, actionNames.Value );

                    if ( allowedMethods.Value.Contains( httpContext.Request.Method ) )
                    {
                        newRequestHandler = ( e, c, m ) => new BadRequestHandler( e, c, m );
                    }
                    else
                    {
                        newRequestHandler = ( e, c, m ) => new MethodNotAllowedHandler( e, c, m, allowedMethods.Value.ToArray() );
                    }
                }
                else if ( versionNeutral.Value )
                {
                    Logger.ApiVersionUnspecified( actionNames.Value );
                    message = SR.VersionNeutralResourceNotSupported.FormatDefault( httpContext.Request.GetDisplayUrl() );

                    if ( allowedMethods.Value.Contains( httpContext.Request.Method ) )
                    {
                        return new BadRequestHandler( ErrorResponseProvider, UnsupportedApiVersion, message );
                    }

                    return new MethodNotAllowedHandler( ErrorResponseProvider, UnsupportedApiVersion, message, allowedMethods.Value.ToArray() );
                }
                else
                {
                    code = InvalidApiVersion;
                    Logger.ApiVersionInvalid( requestedVersion );
                    newRequestHandler = ( e, c, m ) => new BadRequestHandler( e, c, m );
                }
            }
            else
            {
                requestedVersion = parsedVersion.ToString();
                code = UnsupportedApiVersion;
                Logger.ApiVersionUnmatched( parsedVersion, actionNames.Value );

                if ( allowedMethods.Value.Contains( httpContext.Request.Method ) )
                {
                    newRequestHandler = ( e, c, m ) => new BadRequestHandler( e, c, m );
                }
                else
                {
                    newRequestHandler = ( e, c, m ) => new MethodNotAllowedHandler( e, c, m, allowedMethods.Value.ToArray() );
                }
            }

            message = SR.VersionedResourceNotSupported.FormatDefault( httpContext.Request.GetDisplayUrl(), requestedVersion );
            return newRequestHandler( ErrorResponseProvider, code, message );
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