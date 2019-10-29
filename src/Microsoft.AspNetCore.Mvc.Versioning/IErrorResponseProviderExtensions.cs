namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using System;
    using static Microsoft.AspNetCore.Http.StatusCodes;

    static class IErrorResponseProviderExtensions
    {
        internal static IActionResult BadRequest( this IErrorResponseProvider responseProvider, HttpContext context, string code, string message, string? messageDetail = null ) =>
            responseProvider.CreateResponse( new ErrorResponseContext( context.Request, Status400BadRequest, code, message, messageDetail ) );

        internal static IActionResult MethodNotAllowed( this IErrorResponseProvider responseProvider, HttpContext context, string code, string message, string? messageDetail = null ) =>
            responseProvider.CreateResponse( new ErrorResponseContext( context.Request, Status405MethodNotAllowed, code, message, messageDetail ) );
    }
}