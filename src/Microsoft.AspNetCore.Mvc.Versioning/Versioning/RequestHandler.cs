namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using AspNetCore.Routing;
    using Http;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    internal abstract class RequestHandler
    {
        protected RequestHandler( ApiVersioningOptions options, string code, string message )
        {
            Contract.Requires( options != null );
            Contract.Requires( !string.IsNullOrEmpty( message ) );

            Options = options;
            Message = message;
            Code = code;
        }

        protected ApiVersioningOptions Options { get; }

        protected string Code { get; }

        protected string Message { get; }

        protected abstract IActionResult CreateResult( HttpContext context );

        internal Task ExecuteAsync( HttpContext context )
        {
            Contract.Requires( context != null );

            var result = CreateResult( context );
            var actionContext = new ActionContext()
            {
                HttpContext = context,
                RouteData = context.GetRouteData(),
                ActionDescriptor = new ActionDescriptor()
            };

            return result.ExecuteResultAsync( actionContext );
        }

        public static implicit operator RequestDelegate( RequestHandler handler ) => handler.ExecuteAsync;
    }
}