// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0130

namespace System.Net.Http;

using Asp.Versioning;
using Backport;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;

/// <summary>
/// Provides extension methods for the <see cref="HttpRequestMessage"/> class.
/// </summary>
public static class HttpRequestMessageExtensions
{
    private const string RoutingContextKey = "MS_RoutingContext";
    private const string ApiVersionPropertiesKey = "MS_" + nameof( ApiVersionRequestProperties );

    extension( HttpRequestMessage request )
    {
        private HttpResponseMessage CreateErrorResponse( HttpStatusCode statusCode, Func<bool, HttpError> errorCreator )
        {
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

        internal HttpResponseMessage CreateErrorResponse( HttpStatusCode statusCode, string message, string messageDetail )
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
        /// <returns>The current <see cref="ApiVersioningOptions">API versioning options</see>.</returns>
        public ApiVersioningOptions ApiVersioningOptions
        {
            get
            {
                var configuration = request.GetConfiguration();

                if ( configuration == null )
                {
                    configuration = new HttpConfiguration();
                    request.RegisterForDispose( configuration );
                    request.SetConfiguration( configuration );
                }

                return configuration.ApiVersioningOptions;
            }
        }

        /// <summary>
        /// Gets the current API versioning request properties.
        /// </summary>
        /// <returns>The current <see cref="ApiVersionRequestProperties">API versioning properties</see>.</returns>
        public ApiVersionRequestProperties ApiVersionProperties
        {
            get
            {
                ArgumentNullException.ThrowIfNull( request );

                if ( request.Properties.TryGetValue( ApiVersionPropertiesKey, out ApiVersionRequestProperties? properties ) )
                {
                    return properties!;
                }

                var forceRouteConstraintEvaluation = !request.Properties.ContainsKey( RoutingContextKey );

                request.Properties[ApiVersionPropertiesKey] = properties = new( request );

                if ( forceRouteConstraintEvaluation && request.GetConfiguration() is HttpConfiguration configuration )
                {
                    // HACK: do NOT use 'HttpRouteCollection.GetRouteData' because it can result in a LockRecursionException when hosted on IIS
                    // REF: https://github.com/microsoft/referencesource/blob/master/System.Web/Routing/RouteCollection.cs#L159
                    var routes = configuration.Routes;
                    var context = request.GetRequestContext();
                    var virtualPathRoot = context?.VirtualPathRoot ?? routes.VirtualPathRoot ?? string.Empty;

                    // HACK: do NOT use a normal 'for' loop here because the IIS implementation does not support indexing
                    foreach ( var route in routes )
                    {
                        if ( route.GetRouteData( virtualPathRoot, request ) is not null )
                        {
                            break;
                        }
                    }
                }

                return properties;
            }
        }

        /// <summary>
        /// Gets the current service API version requested.
        /// </summary>
        /// <returns>The requested <see cref="ApiVersion">API version</see>.</returns>
        /// <remarks>This method will return <c>null</c> no service API version was requested or the requested
        /// service API version is in an invalid format.</remarks>
        /// <exception cref="AmbiguousApiVersionException">Multiple, different API versions were requested.</exception>
        public ApiVersion? RequestedApiVersion => request.ApiVersionProperties.RequestedApiVersion;

        internal Tuple<MediaTypeHeaderValue, MediaTypeFormatter> GetProblemDetailsResponseType()
        {
            var configuration = request.GetConfiguration();
            var negotiator = configuration.Services.GetContentNegotiator();
            var result = negotiator.Negotiate( typeof( ProblemDetails ), request, configuration.Formatters );

            return result.MediaType.MediaType switch
            {
                null => Tuple.Create(
                    MediaTypeHeaderValue.Parse( ProblemDetailsDefaults.MediaType.Json ),
                    (MediaTypeFormatter) ( configuration.Formatters.JsonFormatter ?? new() ) ),
                "application/xml" => Tuple.Create(
                    MediaTypeHeaderValue.Parse( ProblemDetailsDefaults.MediaType.Xml ),
                    result.Formatter ),
                _ => Tuple.Create(
                    result.MediaType,
                    result.Formatter ),
            };
        }
    }
}