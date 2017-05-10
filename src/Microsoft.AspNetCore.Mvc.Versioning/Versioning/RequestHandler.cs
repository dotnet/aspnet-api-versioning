namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using AspNetCore.Routing;
    using Http;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    abstract class RequestHandler
    {
        protected RequestHandler( IErrorResponseProvider errorResponseProvider, string code, string message )
        {
            Contract.Requires( errorResponseProvider != null );
            Contract.Requires( !string.IsNullOrEmpty( message ) );

            ErrorResponses = errorResponseProvider;
            Message = message;
            Code = code;
        }

        protected IErrorResponseProvider ErrorResponses { get; }

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

        public static implicit operator RequestDelegate( RequestHandler handler ) =>
            handler == null ? default( RequestDelegate ) : handler.ExecuteAsync;
    }
}