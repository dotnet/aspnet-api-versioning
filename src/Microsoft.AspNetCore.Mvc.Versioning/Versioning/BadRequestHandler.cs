namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Abstractions;
    using AspNetCore.Routing;
    using Http;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    internal sealed class BadRequestHandler
    {
        private readonly ApiVersioningOptions options;
        private readonly string code;
        private readonly string message;

        internal BadRequestHandler( ApiVersioningOptions options, string message )
            : this( options, null, message )
        {
        }

        internal BadRequestHandler( ApiVersioningOptions options, string code, string message )
        {
            Contract.Requires( options != null );
            Contract.Requires( !string.IsNullOrEmpty( message ) );

            this.options = options;
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
            var result = options.CreateBadRequest( context.Request, code, message, null );
            await result.ExecuteResultAsync( actionContext );
        }

        public static implicit operator RequestDelegate( BadRequestHandler handler ) => handler.ExecuteAsync;
    }
}