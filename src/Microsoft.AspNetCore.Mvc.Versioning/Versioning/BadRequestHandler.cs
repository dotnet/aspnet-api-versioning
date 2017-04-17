namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Http;

    sealed class BadRequestHandler : RequestHandler
    {
        internal BadRequestHandler( ApiVersioningOptions options, string code, string message )
            : base( options, code, message ) { }

        protected override IActionResult CreateResult( HttpContext context ) =>
            Options.ErrorResponses.BadRequest( context, Code, Message );
    }
}