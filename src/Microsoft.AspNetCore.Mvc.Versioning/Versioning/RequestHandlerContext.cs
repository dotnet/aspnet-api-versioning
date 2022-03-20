namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Collections.Generic;

    sealed class RequestHandlerContext
    {
        readonly IReportApiVersions reporter;
        readonly Lazy<ApiVersionModel> apiVersions;
        string[]? allowedMethods;
        IList<object>? metadata;

        internal RequestHandlerContext(
            IErrorResponseProvider errorResponseProvider,
            IReportApiVersions reportApiVersions )
            : this(
                  errorResponseProvider,
                  reportApiVersions,
                  new Lazy<ApiVersionModel>( () => ApiVersionModel.Empty ) ) { }

        internal RequestHandlerContext(
            IErrorResponseProvider errorResponseProvider,
            IReportApiVersions reportApiVersions,
            Lazy<ApiVersionModel> apiVersions )
        {
            ErrorResponses = errorResponseProvider;
            reporter = reportApiVersions;
            this.apiVersions = apiVersions;
        }

        internal IErrorResponseProvider ErrorResponses { get; }

        internal string Message { get; set; } = string.Empty;

        internal string Code { get; set; } = string.Empty;

        internal string[] AllowedMethods
        {
            get => allowedMethods ?? Array.Empty<string>();
            set => allowedMethods = value;
        }

        internal IList<object> Metadata
        {
            get => metadata ??= new List<object>();
            set => metadata = value;
        }

        internal void ReportApiVersions( HttpResponse response ) =>
            reporter.Report( response.Headers, apiVersions );
    }
}