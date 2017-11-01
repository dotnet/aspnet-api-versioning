namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Tracing;
    using Versioning;
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
            Contract.Requires( request != null );
            Contract.Requires( allApiVersions != null );

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

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseException NewNotFoundOrBadRequestException( ControllerSelectionResult conventionRouteResult, ControllerSelectionResult directRouteResult ) =>
            CreateBadRequest( conventionRouteResult, directRouteResult ) ?? CreateNotFound( conventionRouteResult );

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseMessage CreateBadRequestResponse( ApiVersion requestedVersion )
        {
            var response = requestedVersion == null ?
                           CreateBadRequestForUnspecifiedApiVersionOrInvalidApiVersion( versionNeutral: false ) :
                           CreateBadRequestForUnsupportedApiVersion( requestedVersion );

            ApiVersionReporter.Report( response.Headers, allApiVersions );

            return response;
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseException CreateBadRequest( ApiVersion requestedVersion ) => new HttpResponseException( CreateBadRequestResponse( requestedVersion ) );

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        HttpResponseException CreateBadRequest( ControllerSelectionResult conventionRouteResult, ControllerSelectionResult directRouteResult )
        {
            Contract.Requires( conventionRouteResult != null );

            var requestedVersion = default( ApiVersion );

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

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        HttpResponseMessage CreateBadRequestForUnspecifiedApiVersionOrInvalidApiVersion( bool versionNeutral )
        {
            var requestedVersion = request.ApiVersionProperties().RawApiVersion;
            var message = default( string );

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
            else if ( TryParse( requestedVersion, out var parsedVersion ) )
            {
                return null;
            }

            message = SR.VersionedResourceNotSupported.FormatDefault( request.RequestUri, requestedVersion );
            var messageDetail = SR.VersionedControllerNameNotFound.FormatDefault( request.RequestUri, requestedVersion );

            TraceWriter.Info( request, ControllerSelectorCategory, message );

            return Options.ErrorResponses.BadRequest( request, InvalidApiVersion, message, messageDetail );
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        HttpResponseMessage CreateBadRequestForUnsupportedApiVersion( ApiVersion requestedVersion )
        {
            Contract.Requires( requestedVersion != null );
            Contract.Ensures( Contract.Result<HttpResponseMessage>() != null );

            var message = SR.VersionedResourceNotSupported.FormatDefault( request.RequestUri, requestedVersion );
            var messageDetail = SR.VersionedControllerNameNotFound.FormatDefault( request.RequestUri, requestedVersion );

            TraceWriter.Info( request, ControllerSelectorCategory, message );

            return Options.ErrorResponses.BadRequest( request, UnsupportedApiVersion, message, messageDetail );
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseMessage CreateMethodNotAllowedResponse( bool versionNeutral, IEnumerable<HttpMethod> allowedMethods )
        {
            Contract.Requires( allowedMethods != null );

            var response = CreateBadRequestForUnspecifiedApiVersionOrInvalidApiVersion( versionNeutral );

            if ( response != null )
            {
                ApiVersionReporter.Report( response.Headers, allApiVersions );
                return response;
            }

            var requestedMethod = request.Method;
            var version = request.GetRequestedApiVersion()?.ToString() ?? "(null)";
            var message = default( string );
            var messageDetail = default( string );

            if ( versionNeutral )
            {
                message = SR.VersionedResourceNotSupported.FormatDefault( request.RequestUri, version );
                messageDetail = SR.VersionedControllerNameNotFound.FormatDefault( request.RequestUri, version );
            }
            else
            {
                message = SR.VersionedMethodNotSupported.FormatDefault( version, requestedMethod );
                messageDetail = SR.VersionedActionNameNotFound.FormatDefault( request.RequestUri, requestedMethod, version );
            }

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

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseException NewMethodNotAllowedException( bool versionNeutral, IEnumerable<HttpMethod> allowedMethods ) =>
            new HttpResponseException( CreateMethodNotAllowedResponse( versionNeutral, allowedMethods ) );

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        HttpResponseException CreateNotFound( ControllerSelectionResult conventionRouteResult )
        {
            Contract.Requires( conventionRouteResult != null );

            var message = SR.ResourceNotFound.FormatDefault( request.RequestUri );
            var messageDetail = default( string );

            if ( IsNullOrEmpty( conventionRouteResult.ControllerName ) )
            {
                messageDetail = SR.ControllerNameNotFound.FormatDefault( request.RequestUri );
            }
            else
            {
                messageDetail = SR.DefaultControllerFactory_ControllerNameNotFound.FormatDefault( conventionRouteResult.ControllerName );
            }

            TraceWriter.Info( request, ControllerSelectorCategory, message );

            return new HttpResponseException( request.CreateErrorResponse( NotFound, message, messageDetail ) );
        }
    }
}