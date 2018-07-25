﻿namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Diagnostics.Contracts;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;

    sealed class MethodNotAllowedHandler : RequestHandler
    {
        internal MethodNotAllowedHandler( RequestHandlerContext context ) : base( context ) { }

        protected override IActionResult CreateResult( HttpContext httpContext )
        {
            var result = Context.ErrorResponses.MethodNotAllowed( httpContext, Context.Code, Context.Message );
            var allowedMethods = Context.AllowedMethods;
            return allowedMethods?.Length > 0 ? new AllowHeaderResult( result, allowedMethods ) : result;
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