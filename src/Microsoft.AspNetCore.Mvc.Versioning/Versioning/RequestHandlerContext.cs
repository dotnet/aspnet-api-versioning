namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Diagnostics.Contracts;

    sealed class RequestHandlerContext
    {
        readonly IReportApiVersions reporter;
        readonly Lazy<ApiVersionModel> apiVersions;

        internal RequestHandlerContext( IErrorResponseProvider errorResponseProvider )
            : this( errorResponseProvider, null, null ) { }

        internal RequestHandlerContext(
            IErrorResponseProvider errorResponseProvider,
            IReportApiVersions reportApiVersions,
            Lazy<ApiVersionModel> apiVersions )
        {
            Contract.Requires( errorResponseProvider != null );

            ErrorResponses = errorResponseProvider;
            reporter = reportApiVersions;
            this.apiVersions = apiVersions;
        }

        internal IErrorResponseProvider ErrorResponses { get; }

        internal string Message { get; set; }

        internal string Code { get; set; }

        internal string[] AllowedMethods { get; set; }

        internal void ReportApiVersions( HttpResponse response )
        {
            Contract.Requires( response != null );

            if ( reporter != null && apiVersions != null )
            {
                reporter.Report( response.Headers, apiVersions );
            }
        }
    }
}