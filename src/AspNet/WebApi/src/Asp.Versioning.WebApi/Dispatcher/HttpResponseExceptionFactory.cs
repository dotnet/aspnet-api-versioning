// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Dispatcher;

using System.Globalization;
using System.Net;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Tracing;
using static System.Net.HttpStatusCode;

internal sealed class HttpResponseExceptionFactory
{
    private const string Allow = nameof( Allow );
    private static readonly string ControllerSelectorCategory = typeof( IHttpControllerSelector ).FullName;
    private readonly HttpRequestMessage request;
    private readonly HttpConfiguration configuration;
    private readonly ControllerSelectionContext? context;
    private ApiVersionModel? apiVersionModel;

    internal HttpResponseExceptionFactory( HttpRequestMessage request, ControllerSelectionContext context )
    {
        this.request = request;
        configuration = request.GetConfiguration();
        this.context = context;
    }

    internal HttpResponseExceptionFactory( HttpRequestMessage request, ApiVersionModel apiVersionModel )
    {
        this.request = request;
        configuration = request.GetConfiguration();
        this.apiVersionModel = apiVersionModel;
    }

    private ApiVersionModel AllApiVersions => apiVersionModel ??= context!.AllVersions;

    private ApiVersioningOptions Options => configuration.GetApiVersioningOptions();

    private IProblemDetailsFactory ProblemDetails => configuration.GetProblemDetailsFactory();

    private ITraceWriter TraceWriter => configuration.Services.GetTraceWriter() ?? NullTraceWriter.Instance;

    private IReportApiVersions ApiVersionReporter => configuration.GetApiVersionReporter();

    internal HttpResponseException NewUnmatchedException(
        ControllerSelectionResult conventionRouteResult,
        ControllerSelectionResult? directRouteResult = default )
    {
        var couldMatch = CouldMatchApiVersion( conventionRouteResult, directRouteResult );
        var properties = default( ApiVersionRequestProperties );
        HttpResponseMessage response;

        if ( couldMatch )
        {
            properties = request.ApiVersionProperties();

            if ( properties.RawRequestedApiVersions.Count == 0 )
            {
                response = CreateBadRequestForUnspecifiedApiVersion();
                ApiVersionReporter.Report( response, AllApiVersions );
                return new( response );
            }
        }

        var options = Options;
        var versionsOnlyByMediaType = options.ApiVersionReader.VersionsByMediaType( allowMultipleLocations: false );

        if ( versionsOnlyByMediaType )
        {
            response = CreateUnsupportedMediaType();
        }
        else
        {
            if ( couldMatch )
            {
                properties ??= request.ApiVersionProperties();

                if ( properties.RequestedApiVersion is ApiVersion apiVersion )
                {
                    HttpStatusCode statusCode;
                    var matchedUrlSegment = !string.IsNullOrEmpty( properties.RouteParameter );

                    if ( matchedUrlSegment )
                    {
                        statusCode = NotFound;
                    }
                    else
                    {
                        var versionsByUrlOnly = options.ApiVersionReader.VersionsByUrl( allowMultipleLocations: false );
                        statusCode = versionsByUrlOnly ? NotFound : options.UnsupportedApiVersionStatusCode;
                    }

                    response = CreateResponseForUnsupportedApiVersion( apiVersion, statusCode );
                }
                else
                {
                    response = CreateNotFound( conventionRouteResult );
                }
            }
            else
            {
                response = CreateNotFound( conventionRouteResult );
            }
        }

        if ( couldMatch )
        {
            ApiVersionReporter.Report( response, AllApiVersions );
        }

        return new( response );
    }

    internal HttpResponseMessage CreateBadRequestForInvalidApiVersion()
    {
        var requestedVersion = request.ApiVersionProperties().RawRequestedApiVersion;
        var safeUrl = request.RequestUri.SafeFullPath();
        var detail = string.Format( CultureInfo.CurrentCulture, SR.VersionedResourceNotSupported, safeUrl, requestedVersion );

        TraceWriter.Info( request, ControllerSelectorCategory, detail );

        var (type, title) = ProblemDetailsDefaults.Invalid;
        var problem = ProblemDetails.CreateProblemDetails( request, (int) BadRequest, title, type, detail );
        var (mediaType, formatter) = request.GetProblemDetailsResponseType();

        return request.CreateResponse( BadRequest, problem, formatter, mediaType );
    }

    internal HttpResponseMessage CreateBadRequestForAmbiguousApiVersion( IReadOnlyList<string> apiVersions )
    {
        var detail = string.Format(
            CultureInfo.InvariantCulture,
            CommonSR.MultipleDifferentApiVersionsRequested,
            string.Join( ", ", apiVersions ) );

        TraceWriter.Info( request, ProblemDetailsDefaults.Ambiguous.Code, detail );

        var (type, title) = ProblemDetailsDefaults.Ambiguous;
        var problem = ProblemDetails.CreateProblemDetails( request, (int) BadRequest, title, type, detail );
        var (mediaType, formatter) = request.GetProblemDetailsResponseType();

        return request.CreateResponse( BadRequest, problem, formatter, mediaType );
    }

    private static bool CouldMatchApiVersion( ControllerSelectionResult conventionRouteResult, ControllerSelectionResult? directRouteResult )
    {
        if ( conventionRouteResult.HasCandidates )
        {
            return true;
        }

        if ( directRouteResult is not null && directRouteResult.HasCandidates )
        {
            return true;
        }

        return false;
    }

    private HttpResponseMessage CreateBadRequestForUnspecifiedApiVersion()
    {
        var detail = SR.ApiVersionUnspecified;

        TraceWriter.Info( request, ControllerSelectorCategory, detail );

        var (type, title) = ProblemDetailsDefaults.Unspecified;
        var problem = ProblemDetails.CreateProblemDetails( request, (int) BadRequest, title, type, detail );
        var (mediaType, formatter) = request.GetProblemDetailsResponseType();

        return request.CreateResponse( BadRequest, problem, formatter, mediaType );
    }

    private HttpResponseMessage CreateResponseForUnsupportedApiVersion( ApiVersion requestedVersion, HttpStatusCode statusCode )
    {
        var safeUrl = request.RequestUri.SafeFullPath();
        var detail = string.Format( CultureInfo.CurrentCulture, SR.VersionedResourceNotSupported, safeUrl, requestedVersion );

        TraceWriter.Info( request, ControllerSelectorCategory, detail );

        var (type, title) = ProblemDetailsDefaults.Unsupported;
        var problem = ProblemDetails.CreateProblemDetails( request, (int) statusCode, title, type, detail );
        var (mediaType, formatter) = request.GetProblemDetailsResponseType();

        return request.CreateResponse( statusCode, problem, formatter, mediaType );
    }

    internal HttpResponseMessage CreateMethodNotAllowedResponse( IEnumerable<HttpMethod> allowedMethods )
    {
        var requestedMethod = request.Method;
        var version = request.GetRequestedApiVersion()?.ToString() ?? "(null)";
        var detail = string.Format( CultureInfo.CurrentCulture, SR.VersionedMethodNotSupported, version, requestedMethod );

        TraceWriter.Info( request, ControllerSelectorCategory, detail );

        var (type, title) = ProblemDetailsDefaults.Unsupported;
        var problem = ProblemDetails.CreateProblemDetails( request, (int) MethodNotAllowed, title, type, detail );
        var (mediaType, formatter) = request.GetProblemDetailsResponseType();
        var response = request.CreateResponse( MethodNotAllowed, problem, formatter, mediaType );
        var headers = response.Content.Headers;

        if ( headers.Allow.Count == 0 )
        {
            headers.Allow.AddRange( allowedMethods.Select( m => m.Method ) );
        }

        ApiVersionReporter.Report( response, AllApiVersions );

        return response;
    }

    internal HttpResponseException NewMethodNotAllowedException( IEnumerable<HttpMethod> allowedMethods ) =>
        new( CreateMethodNotAllowedResponse( allowedMethods ) );

    private HttpResponseMessage CreateNotFound( ControllerSelectionResult conventionRouteResult )
    {
        var safeUrl = request.RequestUri.SafeFullPath();
        var culture = CultureInfo.CurrentCulture;
        var message = string.Format( culture, SR.ResourceNotFound, safeUrl );
        var controllerName = conventionRouteResult.ControllerName;
        var messageDetail = string.IsNullOrEmpty( controllerName )
            ? string.Format( culture, SR.ControllerNameNotFound, safeUrl )
            : string.Format( culture, SR.DefaultControllerFactory_ControllerNameNotFound, controllerName );

        TraceWriter.Info( request, ControllerSelectorCategory, message );

        return request.CreateErrorResponse( NotFound, message, messageDetail );
    }

    private HttpResponseMessage CreateUnsupportedMediaType()
    {
        var content = request.Content;
        var statusCode = content != null && content.Headers.ContentType != null ? UnsupportedMediaType : NotAcceptable;
        var version = request.GetRequestedApiVersion()?.ToString() ?? "(null)";
        var detail = string.Format( CultureInfo.CurrentCulture, SR.VersionedMediaTypeNotSupported, version );

        TraceWriter.Info( request, ControllerSelectorCategory, detail );

        var (type, title) = ProblemDetailsDefaults.Unsupported;
        var problem = ProblemDetails.CreateProblemDetails( request, (int) statusCode, title, type, detail );
        var (mediaType, formatter) = request.GetProblemDetailsResponseType();

        return request.CreateResponse( statusCode, problem, formatter, mediaType );
    }
}