namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Routing;
    using System.Threading.Tasks;

    abstract class RequestHandler
    {
        protected RequestHandler( RequestHandlerContext context ) => Context = context;

        protected RequestHandlerContext Context { get; }

        protected abstract IActionResult CreateResult( HttpContext httpContext );

        internal Task ExecuteAsync( HttpContext httpContext )
        {
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

#pragma warning disable CA2225 // Operator overloads have named alternates; implicit cast only intended
        public static implicit operator RequestDelegate?( RequestHandler handler ) =>
            handler == null ? default( RequestDelegate ) : handler.ExecuteAsync;
#pragma warning restore CA2225

        public static implicit operator Endpoint?( RequestHandler? handler ) => handler?.ToEndpoint();

        internal Endpoint ToEndpoint()
        {
            var metadata = Context.Metadata == null ?
                           new EndpointMetadataCollection() :
                           new EndpointMetadataCollection( Context.Metadata );

            return new Endpoint( ExecuteAsync, metadata, default );
        }
    }
}