namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;

    sealed class RequestHandlerContext
    {
        readonly IReportApiVersions? reporter;
        readonly Lazy<ApiVersionModel>? apiVersions;

        internal RequestHandlerContext( IErrorResponseProvider errorResponseProvider )
            : this( errorResponseProvider, null, null ) { }

        internal RequestHandlerContext(
            IErrorResponseProvider errorResponseProvider,
            IReportApiVersions? reportApiVersions,
            Lazy<ApiVersionModel>? apiVersions )
        {
            ErrorResponses = errorResponseProvider;
            reporter = reportApiVersions;
            this.apiVersions = apiVersions;
        }

        internal IErrorResponseProvider ErrorResponses { get; }

        internal string Message { get; set; } = string.Empty;

        internal string Code { get; set; } = string.Empty;

        internal string[] AllowedMethods { get; set; } = Array.Empty<string>();

        internal IList<object> Metadata { get; set; } = Array.Empty<object>();

        internal void ReportApiVersions( HttpResponse response )
        {
            if ( reporter != null && apiVersions != null )
            {
                reporter.Report( response.Headers, apiVersions );
            }
        }
    }
}