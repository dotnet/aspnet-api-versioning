namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using AspNetCore.Routing;
    using Http;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    abstract class RequestHandler
    {
        protected RequestHandler( RequestHandlerContext context ) => Context = context;

        protected RequestHandlerContext Context { get; }

        protected abstract IActionResult CreateResult( HttpContext httpContext );

        internal Task ExecuteAsync( HttpContext httpContext )
        {
            Contract.Requires( httpContext != null );

            var result = CreateResult( httpContext );
            var actionContext = new ActionContext()
            {
                HttpContext = httpContext,
                RouteData = httpContext.GetRouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            Context.ReportApiVersions( httpContext.Response );
            return result.ExecuteResultAsync( actionContext );
        }

        public static implicit operator RequestDelegate( RequestHandler handler ) =>
            handler == null ? default( RequestDelegate ) : handler.ExecuteAsync;
    }
}