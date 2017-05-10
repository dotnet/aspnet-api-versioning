namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Extensions.Primitives;
    using Http;
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;

    sealed class MethodNotAllowedHandler : RequestHandler
    {
        readonly string[] allowedMethods;

        internal MethodNotAllowedHandler( IErrorResponseProvider errorResponseProvider, string code, string message, string[] allowedMethods )
            : base( errorResponseProvider, code, message )
        {
            Contract.Requires( allowedMethods != null );
            this.allowedMethods = allowedMethods;
        }

        protected override IActionResult CreateResult( HttpContext context )
        {
            var result = ErrorResponses.MethodNotAllowed( context, Code, Message );
            return allowedMethods.Length == 0 ? result : new AllowHeaderResult( result, allowedMethods );
        }

        sealed class AllowHeaderResult : IActionResult
        {
            const string Allow = nameof( Allow );
            readonly IActionResult inner;
            readonly string[] allowedMethods;

            internal AllowHeaderResult( IActionResult inner, string[] allowedMethods )
            {
                Contract.Requires( inner != null );
                Contract.Requires( allowedMethods != null );

                this.inner = inner;
                this.allowedMethods = allowedMethods;
            }

            public Task ExecuteResultAsync( ActionContext context )
            {
                var headers = context.HttpContext.Response?.Headers;

                if ( headers != null && !headers.ContainsKey( Allow ) )
                {
                    headers.Add( Allow, new StringValues( allowedMethods ) );
                }

                return inner.ExecuteResultAsync( context );
            }
        }
    }
}