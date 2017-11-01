#if WEBAPI
namespace Microsoft.Web.Http.Versioning
#else
namespace Microsoft.AspNetCore.Mvc.Versioning
#endif
{
    using System;

    /// <summary>
    /// Defines the standard error codes returned in responses related to API versioning.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Indicates that the API version requested by the client is not supported.
        /// </summary>
        public const string UnsupportedApiVersion = nameof( UnsupportedApiVersion );

        /// <summary>
        /// Indicates that an API version is required, but was not specified by the client.
        /// </summary>
        public const string ApiVersionUnspecified = nameof( ApiVersionUnspecified );

        /// <summary>
        /// Indicates that API version requested by the client is invalid or malformed.
        /// </summary>
        public const string InvalidApiVersion = nameof( InvalidApiVersion );

        /// <summary>
        /// Indicates that the client specified an API version multiple times and with different values.
        /// </summary>
        public const string AmbiguousApiVersion = nameof( AmbiguousApiVersion );
    }
}