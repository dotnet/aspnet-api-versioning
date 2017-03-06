namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Tracing;
    using Versioning;
    using static ApiVersion;
    using static System.Net.HttpStatusCode;
    using static System.String;

    internal sealed class HttpResponseExceptionFactory
    {
        const string Allow = nameof( Allow );
        private static readonly string ControllerSelectorCategory = typeof( IHttpControllerSelector ).FullName;
        private readonly HttpRequestMessage request;

        internal HttpResponseExceptionFactory( HttpRequestMessage request )
        {
            Contract.Requires( request != null );
            this.request = request;
        }

        private ITraceWriter TraceWriter => request.GetConfiguration().Services.GetTraceWriter() ?? NullTraceWriter.Instance;

        private ApiVersioningOptions Options => request.GetApiVersioningOptions();

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseException NewNotFoundOrBadRequestException( ControllerSelectionResult conventionRouteResult, ControllerSelectionResult directRouteResult ) =>
            CreateBadRequest( conventionRouteResult, directRouteResult ) ?? CreateNotFound( conventionRouteResult );

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseMessage CreateBadRequestResponse( ApiVersion requestedVersion ) => requestedVersion == null ?
                                                                                                CreateBadRequestForUnspecifiedApiVersionOrInvalidApiVersion( versionNeutral: false ) :
                                                                                                CreateBadRequestForUnsupportedApiVersion( requestedVersion );

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseException CreateBadRequest( ApiVersion requestedVersion ) => new HttpResponseException( CreateBadRequestResponse( requestedVersion ) );

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        private HttpResponseException CreateBadRequest( ControllerSelectionResult conventionRouteResult, ControllerSelectionResult directRouteResult )
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
        private HttpResponseMessage CreateBadRequestForUnspecifiedApiVersionOrInvalidApiVersion( bool versionNeutral )
        {
            var requestedVersion = request.ApiVersionProperties().RawApiVersion;
            var parsedVersion = default( ApiVersion );
            var message = default( string );
            var context = default( ErrorResponseContext );

            if ( IsNullOrEmpty( requestedVersion ) )
            {
                if ( versionNeutral )
                {
                    return null;
                }

                message = SR.ApiVersionUnspecified;
                TraceWriter.Info( request, ControllerSelectorCategory, message );
                context = new ErrorResponseContext( request, "ApiVersionUnspecified", message, messageDetail: null );
                return Options.ErrorResponses.BadRequest( context );
            }
            else if ( TryParse( requestedVersion, out parsedVersion ) )
            {
                return null;
            }

            message = SR.VersionedResourceNotSupported.FormatDefault( request.RequestUri, requestedVersion );
            var messageDetail = SR.VersionedControllerNameNotFound.FormatDefault( request.RequestUri, requestedVersion );
            context = new ErrorResponseContext( request, "InvalidApiVersion", message, messageDetail );

            TraceWriter.Info( request, ControllerSelectorCategory, message );

            return Options.ErrorResponses.BadRequest( context );
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        private HttpResponseMessage CreateBadRequestForUnsupportedApiVersion( ApiVersion requestedVersion )
        {
            Contract.Requires( requestedVersion != null );
            Contract.Ensures( Contract.Result<HttpResponseMessage>() != null );

            var message = SR.VersionedResourceNotSupported.FormatDefault( request.RequestUri, requestedVersion );
            var messageDetail = SR.VersionedControllerNameNotFound.FormatDefault( request.RequestUri, requestedVersion );
            var context = new ErrorResponseContext( request, "UnsupportedApiVersion", message, messageDetail );

            TraceWriter.Info( request, ControllerSelectorCategory, message );

            return Options.ErrorResponses.BadRequest( context );
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseMessage CreateMethodNotAllowedResponse( bool versionNeutral, IEnumerable<HttpMethod> allowedMethods )
        {
            Contract.Requires( allowedMethods != null );

            var response = CreateBadRequestForUnspecifiedApiVersionOrInvalidApiVersion( versionNeutral );

            if ( response != null )
            {
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

            var context = new ErrorResponseContext( request, "UnsupportedApiVersion", message, messageDetail );

            response = Options.ErrorResponses.MethodNotAllowed( context );

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

            return response;
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseException NewMethodNotAllowedException( bool versionNeutral, IEnumerable<HttpMethod> allowedMethods ) =>
            new HttpResponseException( CreateMethodNotAllowedResponse( versionNeutral, allowedMethods ) );

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        private HttpResponseException CreateNotFound( ControllerSelectionResult conventionRouteResult )
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