namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using AspNetCore.Mvc;
    using Http;
    using System;

    /// <summary>
    /// Represents the function invoked to create a HTTP 400 (Bad Request) response related to API versioning.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequest">HTTP request</see>.</param>
    /// <param name="code">The associated error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="messageDetail">The detailed error message, if any.</param>
    /// <returns>A <see cref="BadRequestObjectResult">HTTP response</see> representing for status code 400 (Bad Request).</returns>
    [CLSCompliant( false )]
    public delegate BadRequestObjectResult CreateBadRequestDelegate( HttpRequest request, string code, string message, string messageDetail );
}