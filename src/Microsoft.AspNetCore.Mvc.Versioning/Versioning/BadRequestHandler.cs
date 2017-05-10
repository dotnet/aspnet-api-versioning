namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Http;

    sealed class BadRequestHandler : RequestHandler
    {
        internal BadRequestHandler( IErrorResponseProvider errorResponseProvider, string code, string message )
            : base( errorResponseProvider, code, message ) { }

        protected override IActionResult CreateResult( HttpContext context ) =>
            ErrorResponses.BadRequest( context, Code, Message );
    }
}