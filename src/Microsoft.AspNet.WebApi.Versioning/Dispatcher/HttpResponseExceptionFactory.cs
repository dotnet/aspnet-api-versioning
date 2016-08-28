namespace Microsoft.Web.Http.Dispatcher
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Tracing;
    using static ApiVersion;
    using static System.Net.HttpStatusCode;

    internal sealed class HttpResponseExceptionFactory
    {
        private static readonly string ControllerSelectorCategory = typeof( IHttpControllerSelector ).FullName;
        private readonly HttpRequestMessage request;
        private readonly ITraceWriter traceWriter;

        internal HttpResponseExceptionFactory( HttpRequestMessage request )
        {
            Contract.Requires( request != null );

            this.request = request;
            traceWriter = request.GetConfiguration().Services.GetTraceWriter() ?? NullTraceWriter.Instance;
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        internal HttpResponseException NewNotFoundOrBadRequestException( ControllerSelectionResult conventionRouteResult, ControllerSelectionResult directRouteResult ) =>
            CreateBadRequestForUnsupportedApiVersion( conventionRouteResult, directRouteResult ) ?? CreateBadRequestForInvalidApiVersion() ?? CreateNotFound( conventionRouteResult );

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        private HttpResponseException CreateBadRequestForUnsupportedApiVersion( ControllerSelectionResult conventionRouteResult, ControllerSelectionResult directRouteResult )
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

            var message = SR.VersionedResourceNotSupported.FormatDefault( request.RequestUri, requestedVersion );
            var messageDetail = SR.VersionedControllerNameNotFound.FormatDefault( request.RequestUri, requestedVersion );
            var error = new HttpError() { Message = message, MessageDetail = messageDetail };

            error["Code"] = "UnsupportedApiVersion";
            traceWriter.Info( request, ControllerSelectorCategory, message );

            return new HttpResponseException( request.CreateErrorResponse( BadRequest, error ) );
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        private HttpResponseException CreateBadRequestForInvalidApiVersion()
        {
            var requestedVersion = request.GetRawRequestedApiVersion();
            var parsedVersion = default( ApiVersion );

            if ( string.IsNullOrEmpty( requestedVersion ) || TryParse( requestedVersion, out parsedVersion ) )
            {
                return null;
            }

            var message = SR.VersionedResourceNotSupported.FormatDefault( request.RequestUri, requestedVersion );
            var messageDetail = SR.VersionedControllerNameNotFound.FormatDefault( request.RequestUri, requestedVersion );
            var error = new HttpError() { Message = message, MessageDetail = messageDetail };

            error["Code"] = "InvalidApiVersion";
            traceWriter.Info( request, ControllerSelectorCategory, message );

            return new HttpResponseException( request.CreateErrorResponse( BadRequest, error ) );
        }

        [SuppressMessage( "Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Created exception cannot be disposed. Handled by the caller." )]
        private HttpResponseException CreateNotFound( ControllerSelectionResult conventionRouteResult )
        {
            Contract.Requires( conventionRouteResult != null );

            var message = SR.ResourceNotFound.FormatDefault( request.RequestUri );
            var messageDetail = default( string );

            if ( string.IsNullOrEmpty( conventionRouteResult.ControllerName ) )
            {
                messageDetail = SR.ControllerNameNotFound.FormatDefault( request.RequestUri );
            }
            else
            {
                messageDetail = SR.DefaultControllerFactory_ControllerNameNotFound.FormatDefault( conventionRouteResult.ControllerName );
            }

            traceWriter.Info( request, ControllerSelectorCategory, message );

            return new HttpResponseException( request.CreateErrorResponse( NotFound, message, messageDetail ) );
        }
    }
}
