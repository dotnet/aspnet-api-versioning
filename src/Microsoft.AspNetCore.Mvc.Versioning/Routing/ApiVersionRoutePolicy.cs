namespace Microsoft.AspNetCore.Mvc.Routing
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Routing;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the default API versioning route policy.
    /// </summary>
    [CLSCompliant( false )]
    public class ApiVersionRoutePolicy : IApiVersionRoutePolicy
    {
        readonly IActionContextAccessor actionContextAccessor;
        readonly IActionInvokerFactory actionInvokerFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actionInvokerFactory"></param>
        public ApiVersionRoutePolicy( IActionInvokerFactory actionInvokerFactory ) : this( actionInvokerFactory, null ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiVersionRoutePolicy"/> class.
        /// </summary>
        /// <param name="actionInvokerFactory">The underlying <see cref="IActionInvokerFactory">action invoker factory</see>.</param>
        /// <param name="actionContextAccessor">The associated <see cref="IActionContextAccessor">action context accessor</see>.</param>
        public ApiVersionRoutePolicy( IActionInvokerFactory actionInvokerFactory, IActionContextAccessor actionContextAccessor )
        {
            Arg.NotNull( actionInvokerFactory, nameof( actionInvokerFactory ) );

            this.actionContextAccessor = actionContextAccessor;
            this.actionInvokerFactory = actionInvokerFactory;
        }

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

            switch ( selectionResult.MatchingActions.Count )
            {
                case 0:
                    OnUnmatched( context, selectionResult );
                    break;
                case 1:
                    OnSingleMatch( context, selectionResult, selectionResult.MatchingActions.First() );
                    break;
                default:
                    OnMultipleMatches( context, selectionResult );
                    break;
            }

            return Task.FromResult( false );
        }

        protected virtual void OnSingleMatch( RouteContext context, ApiVersionSelectionResult selectionResult, ActionDescriptor actionDescriptor )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );
            Arg.NotNull( actionDescriptor, nameof( actionDescriptor ) );

            var handler = new DefaultActionHandler( actionInvokerFactory, actionContextAccessor, actionDescriptor );

            actionDescriptor.AggregateAllVersions( selectionResult.MatchingActions );
            context.HttpContext.ApiVersionProperties().ApiVersion = selectionResult.RequestedVersion;
            context.Handler = handler.Invoke;
        }

        protected virtual void OnUnmatched( RouteContext context, ApiVersionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );
        }

        protected virtual void OnMultipleMatches( RouteContext context, ApiVersionSelectionResult selectionResult )
        {
            Arg.NotNull( context, nameof( context ) );
            Arg.NotNull( selectionResult, nameof( selectionResult ) );
        }

        sealed class DefaultActionHandler
        {
            readonly IActionContextAccessor actionContextAccessor;
            readonly IActionInvokerFactory actionInvokerFactory;
            readonly ActionDescriptor actionDescriptor;

            internal DefaultActionHandler(
                IActionInvokerFactory actionInvokerFactory,
                IActionContextAccessor actionContextAccessor,
                ActionDescriptor actionDescriptor )
            {
                this.actionContextAccessor = actionContextAccessor;
                this.actionInvokerFactory = actionInvokerFactory;
                this.actionDescriptor = actionDescriptor;
            }

            internal Task Invoke( HttpContext context )
            {
                Contract.Requires( context != null );

                var routeData = c.GetRouteData();
                var actionContext = new ActionContext( context, routeData, actionDescriptor );

                if ( actionContextAccessor != null )
                {
                    actionContextAccessor.ActionContext = actionContext;
                }

                var invoker = actionInvokerFactory.CreateInvoker( actionContext );

                if ( invoker == null )
                {
                    throw new InvalidOperationException();
                    //throw new InvalidOperationException( Resources.FormatActionInvokerFactory_CouldNotCreateInvoker( actionDescriptor.DisplayName ) );
                }

                return invoker.InvokeAsync();
            }
        }
    }
}