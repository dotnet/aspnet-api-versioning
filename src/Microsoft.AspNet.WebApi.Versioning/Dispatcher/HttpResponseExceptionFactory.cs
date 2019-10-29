namespace Microsoft.Web.Http.Dispatcher
{
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Tracing;
    using static ApiVersion;
    using static System.Net.HttpStatusCode;
    using static System.String;
    using static Versioning.ErrorCodes;

    sealed class HttpResponseExceptionFactory
    {
        const string Allow = nameof( Allow );
        static readonly string ControllerSelectorCategory = typeof( IHttpControllerSelector ).FullName;
        readonly HttpRequestMessage request;
        readonly Lazy<ApiVersionModel> allApiVersions;

        internal HttpResponseExceptionFactory( HttpRequestMessage request, Lazy<ApiVersionModel> allApiVersions )
        {
            this.request = request;
            this.allApiVersions = allApiVersions;
        }

        ApiVersioningOptions Options => request.GetApiVersioningOptions();

        ITraceWriter TraceWriter => request.GetConfiguration().Services.GetTraceWriter() ?? NullTraceWriter.Instance;

        IReportApiVersions ApiVersionReporter
        {
            get
            {
                var dependencyResolver = request.GetConfiguration().DependencyResolver;
                var reporter = (IReportApiVersions) dependencyResolver.GetService( typeof( IReportApiVersions ) );

                if ( reporter == null )
                {
                    reporter = Options.ReportApiVersions ? DefaultApiVersionReporter.Instance : DoNotReportApiVersions.Instance;
                }

                return reporter;
            }
        }

        internal HttpResponseException NewNotFoundOrBadRequestException( ControllerSelectionResult conventionRouteResult, ControllerSelectionResult? directRouteResult ) =>
            CreateBadRequest( conventionRouteResult, directRouteResult ) ?? CreateNotFound( conventionRouteResult );

        internal HttpResponseMessage? CreateBadRequestResponse( ApiVersion? requestedVersion )
        {
            var response = requestedVersion == null ?
                           CreateBadRequestForUnspecifiedApiVersionOrInvalidApiVersion( versionNeutral: false ) :
                           CreateBadRequestForUnsupportedApiVersion( requestedVersion );

            if ( response != null )
            {
                ApiVersionReporter.Report( response.Headers, allApiVersions );
            }

            return response;
        }

        internal HttpResponseException CreateBadRequest( ApiVersion? requestedVersion ) => new HttpResponseException( CreateBadRequestResponse( requestedVersion ) );

        HttpResponseException? CreateBadRequest( ControllerSelectionResult conventionRouteResult, ControllerSelectionResult? directRouteResult )
        {
            ApiVersion? requestedVersion;

            if ( conventionRouteResult.CouldMatchVersion )
            {
                requestedVersion = conventionRouteResult.RequestedVersion;
            }
            else if ( directRouteResult != null && directRouteResult.CouldMatchVersion )
            {
                requestedVersion = directRouteResult.RequestedVersion;
            }
            else
            {
                return null;
            }

            return CreateBadRequest( requestedVersion );
        }

        HttpResponseMessage? CreateBadRequestForUnspecifiedApiVersionOrInvalidApiVersion( bool versionNeutral )
        {
            var requestedVersion = request.ApiVersionProperties().RawRequestedApiVersion;
            string message;

            if ( IsNullOrEmpty( requestedVersion ) )
            {
                if ( versionNeutral )
                {
                    return null;
                }

                message = SR.ApiVersionUnspecified;
                TraceWriter.Info( request, ControllerSelectorCategory, message );
                return Options.ErrorResponses.BadRequest( request, ApiVersionUnspecified, message );
            }
            else if ( TryParse( requestedVersion, out _ ) )
            {
                return null;
            }

            message = SR.VersionedResourceNotSupported.FormatDefault( request.RequestUri, requestedVersion );
            var messageDetail = SR.VersionedControllerNameNotFound.FormatDefault( request.RequestUri, requestedVersion );

            TraceWriter.Info( request, ControllerSelectorCategory, message );

            return Options.ErrorResponses.BadRequest( request, InvalidApiVersion, message, messageDetail );
        }

        HttpResponseMessage CreateBadRequestForUnsupportedApiVersion( ApiVersion requestedVersion )
        {
            var safeUrl = request.RequestUri.SafeFullPath();
            var message = SR.VersionedResourceNotSupported.FormatDefault( safeUrl, requestedVersion );
            var messageDetail = SR.VersionedControllerNameNotFound.FormatDefault( safeUrl, requestedVersion );

            TraceWriter.Info( request, ControllerSelectorCategory, message );

            return Options.ErrorResponses.BadRequest( request, UnsupportedApiVersion, message, messageDetail );
        }

        internal HttpResponseMessage CreateMethodNotAllowedResponse( bool versionNeutral, IEnumerable<HttpMethod> allowedMethods )
        {
            var response = CreateBadRequestForUnspecifiedApiVersionOrInvalidApiVersion( versionNeutral || Options.AssumeDefaultVersionWhenUnspecified );

            if ( response != null )
            {
                ApiVersionReporter.Report( response.Headers, allApiVersions );
                return response;
            }

            var requestedMethod = request.Method;
            var version = request.GetRequestedApiVersion()?.ToString() ?? "(null)";
            var message = SR.VersionedMethodNotSupported.FormatDefault( version, requestedMethod );
            var safeUrl = request.RequestUri.SafeFullPath();

            var messageDetail = SR.VersionedActionNameNotFound.FormatDefault( safeUrl, requestedMethod, version );

            TraceWriter.Info( request, ControllerSelectorCategory, message );
            response = Options.ErrorResponses.MethodNotAllowed( request, UnsupportedApiVersion, message, messageDetail );

            if ( response.Content == null )
            {
                response.Content = new StringContent( Empty );
                response.Content.Headers.ContentType = null;
            }

            var headers = response.Content.Headers;

            if ( headers.Allow.Count == 0 )
            {
                headers.Allow.AddRange( allowedMethods.Select( m => m.Method ) );
            }

            ApiVersionReporter.Report( response.Headers, allApiVersions );

            return response;
        }

        internal HttpResponseException NewMethodNotAllowedException( bool versionNeutral, IEnumerable<HttpMethod> allowedMethods ) =>
            new HttpResponseException( CreateMethodNotAllowedResponse( versionNeutral, allowedMethods ) );

        HttpResponseException CreateNotFound( ControllerSelectionResult conventionRouteResult )
        {
            var safeUrl = request.RequestUri.SafeFullPath();
            var message = SR.ResourceNotFound.FormatDefault( safeUrl );
            string messageDetail;

            if ( IsNullOrEmpty( conventionRouteResult.ControllerName ) )
            {
                messageDetail = SR.ControllerNameNotFound.FormatDefault( safeUrl );
            }
            else
            {
                messageDetail = SR.DefaultControllerFactory_ControllerNameNotFound.FormatDefault( conventionRouteResult.ControllerName );
            }

            TraceWriter.Info( request, ControllerSelectorCategory, message );

#pragma warning disable CA2000 // Dispose objects before losing scope
            return new HttpResponseException( request.CreateErrorResponse( NotFound, message, messageDetail ) );
#pragma warning restore CA2000 // Dispose objects before losing scope
        }
    }
}