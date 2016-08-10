namespace System.Web.Http
{
    using ComponentModel;
    using Diagnostics.CodeAnalysis;
    using Diagnostics.Contracts;
    using Microsoft;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using Net;
    using Net.Http;
    using System;
    using static Microsoft.Web.Http.ApiVersion;
    using static ComponentModel.EditorBrowsableState;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        private const string ApiVersionKey = "MS_" + nameof( ApiVersion );

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Handled by the caller." )]
        private static HttpResponseMessage CreateErrorResponse( this HttpRequestMessage request, HttpStatusCode statusCode, Func<bool, HttpError> errorCreator )
        {
            Contract.Requires( request != null );
            Contract.Requires( errorCreator != null );
            Contract.Ensures( Contract.Result<HttpResponseMessage>() != null );

            var configuration = request.GetConfiguration();
            var error = errorCreator( request.ShouldIncludeErrorDetail() );

            if ( configuration == null )
            {
                configuration = new HttpConfiguration();
                request.RegisterForDispose( configuration );
                request.SetConfiguration( configuration );
            }

            return request.CreateResponse( statusCode, error, configuration );
        }

        internal static HttpResponseMessage CreateErrorResponse( this HttpRequestMessage request, HttpStatusCode statusCode, string message, string messageDetail )
        {
            Contract.Requires( request != null );

            return request.CreateErrorResponse(
                statusCode,
                includeErrorDetail =>
                {
                    var error = new HttpError( message );

                    if ( includeErrorDetail )
                    {
                        error.MessageDetail = messageDetail;
                    }

                    return error;
                } );
        }

        /// <summary>
        /// Gets the current API versioning options.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to get the API versioning options for.</param>
        /// <returns>The current <see cref="ApiVersioningOptions">API versioning options</see>.</returns>
        public static ApiVersioningOptions GetApiVersioningOptions( this HttpRequestMessage request )
        {
            Arg.NotNull( request, nameof( request ) );
            Contract.Ensures( Contract.Result<ApiVersioningOptions>() != null );

            return request.GetConfiguration()?.GetApiVersioningOptions() ?? new ApiVersioningOptions();
        }

        /// <summary>
        /// Gets the current raw, unparsed service API version requested.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to get the API version for.</param>
        /// <returns>The raw, unparsed service API version or <c>null</c> if no service API version was requested.</returns>
        /// <remarks>This method is primarily meant for internal use and is generally only useful for instrumentation purposes.
        /// It is recommended that you use the <see cref="GetRequestedApiVersion(HttpRequestMessage)"/> instead.</remarks>
        [EditorBrowsable( Never )]
        public static string GetRawRequestedApiVersion( this HttpRequestMessage request )
        {
            Arg.NotNull( request, nameof( request ) );

            var reader = request.GetApiVersioningOptions().ApiVersionReader;
            return reader.Read( request );
        }

        /// <summary>
        /// Gets the current service API version requested.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to get the API version for.</param>
        /// <returns>The requested <see cref="ApiVersion">API version</see>.</returns>
        /// <remarks>This method will return <c>null</c> no service API version was requested or the requested
        /// service API version is in an invalid format.</remarks>
        [SuppressMessage( "Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated by a code contract." )]
        public static ApiVersion GetRequestedApiVersion( this HttpRequestMessage request )
        {
            Arg.NotNull( request, nameof( request ) );

            var version = default( ApiVersion );

            if ( request.Properties.TryGetValue( ApiVersionKey, out version ) )
            {
                return version;
            }

            var value = request.GetRawRequestedApiVersion();

            if ( TryParse( value, out version ) )
            {
                request.Properties[ApiVersionKey] = version;
                return version;
            }

            request.Properties[ApiVersionKey] = null;
            return null;
        }

        /// <summary>
        /// Gets the current service API version requested.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to get the API version for.</param>
        /// <param name="version">The <see cref="ApiVersion">API version</see> to be set as the requested value.</param>
        /// <remarks>This method is for internal use and is not meant to be called directly in your code.</remarks>
        [EditorBrowsable( Never )]
        public static void SetRequestedApiVersion( this HttpRequestMessage request, ApiVersion version )
        {
            Arg.NotNull( request, nameof( request ) );

            if ( version == null )
            {
                request.Properties.Remove( ApiVersionKey );
            }
            else
            {
                request.Properties[ApiVersionKey] = version;
            }
        }
    }
}