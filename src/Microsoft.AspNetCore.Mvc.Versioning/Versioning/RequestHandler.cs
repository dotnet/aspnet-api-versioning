namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Routing;

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
                ActionDescriptor = new ActionDescriptor(),
            };

            Context.ReportApiVersions( httpContext.Response );
            return result.ExecuteResultAsync( actionContext );
        }

#pragma warning disable CA2225 // Operator overloads have named alternates; intentionally one-way
        public static implicit operator RequestDelegate( RequestHandler handler ) =>
            handler == null ? default( RequestDelegate ) : handler.ExecuteAsync;
#pragma warning restore CA2225
    }
}