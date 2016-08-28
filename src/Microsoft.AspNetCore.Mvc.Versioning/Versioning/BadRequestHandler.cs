namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using AspNetCore.Routing;
    using Http;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    internal sealed class BadRequestHandler
    {
        private readonly string code;
        private readonly string message;

        internal BadRequestHandler( string message )
            : this( null, message )
        {
        }

        internal BadRequestHandler( string code, string message )
        {
            Contract.Requires( !string.IsNullOrEmpty( message ) );
            this.message = message;
            this.code = code;
        }

        internal async Task ExecuteAsync( HttpContext context )
        {
            Contract.Assume( context != null );

            var actionContext = new ActionContext()
            {
                HttpContext = context,
                RouteData = context.GetRouteData(),
                ActionDescriptor = new ActionDescriptor()
            };
            var result = new BadRequestObjectResult( new { Code = code, Message = message } );
            await result.ExecuteResultAsync( actionContext );
        }

        public static implicit operator RequestDelegate( BadRequestHandler handler ) => handler.ExecuteAsync;
    }
}
