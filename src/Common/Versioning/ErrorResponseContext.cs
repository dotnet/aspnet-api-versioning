#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    /// <summary>
    /// Represents the contextual information used when generating HTTP error responses related to API versioning.
    /// </summary>
    public partial class ErrorResponseContext
    {
        /// <summary>
        /// Gets the associated error code.
        /// </summary>
        /// <value>The associated error code.</value>
        public string Code { get; }

        /// <summary>
        /// Gets the associated error message.
        /// </summary>
        /// <value>The error message.</value>
        public string Message { get; }

        /// <summary>
        /// Gets the detailed error message.
        /// </summary>
        /// <value>The detailed error message, if any.</value>
        public string MessageDetail { get; }
    }
}