namespace Microsoft.Web.Http.Versioning
{
    using System;
    using System.Net;
    using System.Net.Http;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Web API.
    /// </content>
    public partial class ErrorResponseContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponseContext"/> class.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequestMessage">HTTP request</see>.</param>
        /// <param name="statusCode">The associated <see cref="HttpStatusCode">HTTP status code</see>.</param>
        /// <param name="errorCode">The associated error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="messageDetail">The detailed error message, if any.</param>
        public ErrorResponseContext( HttpRequestMessage request, HttpStatusCode statusCode, string errorCode, string message, string? messageDetail )
        {
            Request = request;
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Message = message;
            MessageDetail = messageDetail;
        }

        /// <summary>
        /// Gets the current HTTP request.
        /// </summary>
        /// <value>The current <see cref="HttpRequestMessage">HTTP request</see>.</value>
        public HttpRequestMessage Request { get; }
    }
}