namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Net.Http;
    using System.Web.Http;

    /// <summary>
    /// Represents the function invoked to create a HTTP 400 (Bad Request) response related to API versioning.
    /// </summary>
    /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
    /// <param name="code">The associated error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="messageDetail">The detailed error message, if any.</param>
    /// <returns>A <see cref="HttpResponseMessage">HTTP response</see> representing for status code 400 (Bad Request).</returns>
    public delegate HttpResponseMessage CreateBadRequestDelegate( HttpRequestMessage request, string code, string message, string messageDetail );
}