namespace System.Web.Http
{
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net;
    using System.Net.Http;

    /// <summary>
    /// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        const string ApiVersionPropertiesKey = "MS_" + nameof( ApiVersionRequestProperties );

        static HttpResponseMessage CreateErrorResponse( this HttpRequestMessage request, HttpStatusCode statusCode, Func<bool, HttpError> errorCreator )
        {
            var configuration = request.GetConfiguration();
            var error = errorCreator( request.ShouldIncludeErrorDetail() );

            if ( configuration == null )
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                configuration = new HttpConfiguration();
#pragma warning restore CA2000 // Dispose objects before losing scope
                request.RegisterForDispose( configuration );
                request.SetConfiguration( configuration );
            }

            return request.CreateResponse( statusCode, error, configuration );
        }

        internal static HttpResponseMessage CreateErrorResponse( this HttpRequestMessage request, HttpStatusCode statusCode, string message, string messageDetail )
        {
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
            var configuration = request.GetConfiguration();

            if ( configuration == null )
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                configuration = new HttpConfiguration();
#pragma warning restore CA2000 // Dispose objects before losing scope
                request.RegisterForDispose( configuration );
                request.SetConfiguration( configuration );
            }

            return configuration.GetApiVersioningOptions();
        }

        /// <summary>
        /// Gets the current API versioning request properties.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to get the API versioning properties for.</param>
        /// <returns>The current <see cref="ApiVersionRequestProperties">API versioning properties</see>.</returns>
        public static ApiVersionRequestProperties ApiVersionProperties( this HttpRequestMessage request )
        {
            if ( request == null )
            {
                throw new ArgumentNullException( nameof( request ) );
            }

            if ( !request.Properties.TryGetValue( ApiVersionPropertiesKey, out ApiVersionRequestProperties properties ) )
            {
                request.Properties[ApiVersionPropertiesKey] = properties = new ApiVersionRequestProperties( request );
            }

            return properties;
        }

        /// <summary>
        /// Gets the current service API version requested.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage">request</see> to get the API version for.</param>
        /// <returns>The requested <see cref="ApiVersion">API version</see>.</returns>
        /// <remarks>This method will return <c>null</c> no service API version was requested or the requested
        /// service API version is in an invalid format.</remarks>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public static ApiVersion? GetRequestedApiVersion( this HttpRequestMessage request ) => request.ApiVersionProperties().RequestedApiVersion;
    }
}