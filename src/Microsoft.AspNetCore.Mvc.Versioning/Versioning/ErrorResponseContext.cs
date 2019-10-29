namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using System;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    public partial class ErrorResponseContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResponseContext"/> class.
        /// </summary>
        /// <param name="request">The current <see cref="HttpRequest">HTTP request</see>.</param>
        /// <param name="statusCode">The associated HTTP status code.</param>
        /// <param name="errorCode">The associated error code.</param>
        /// <param name="message">The error message.</param>
        /// <param name="messageDetail">The detailed error message, if any.</param>
        [CLSCompliant( false )]
        public ErrorResponseContext( HttpRequest request, int statusCode, string errorCode, string message, string? messageDetail )
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
        /// <value>The current <see cref="HttpRequest">HTTP request</see>.</value>
        [CLSCompliant( false )]
        public HttpRequest Request { get; }
    }
}