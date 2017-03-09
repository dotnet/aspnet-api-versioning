namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Http;

    sealed class BadRequestHandler : RequestHandler
    {
        internal BadRequestHandler( ApiVersioningOptions options, string code, string message )
            : base( options, code, message ) { }

        protected override IActionResult CreateResult( HttpContext context )
        {
            var errorContext = new ErrorResponseContext( context.Request, Code, Message, messageDetail: null );
            return Options.ErrorResponses.BadRequest( errorContext );
        }
    }
}