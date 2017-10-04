namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Http;

    sealed class BadRequestHandler : RequestHandler
    {
        internal BadRequestHandler( RequestHandlerContext context ) : base( context ) { }

        protected override IActionResult CreateResult( HttpContext httpContext ) =>
            Context.ErrorResponses.BadRequest( httpContext, Context.Code, Context.Message );
    }
}