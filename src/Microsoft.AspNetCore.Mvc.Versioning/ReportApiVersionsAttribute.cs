namespace Microsoft.AspNetCore.Mvc
{
    using Abstractions;
    using ApplicationModels;
    using Filters;
    using System;
    using Versioning;

    /// <content>
    /// Provides additional implementation specific to ASP.NET Core.
    /// </content>
    [CLSCompliant( false )]
    public partial class ReportApiVersionsAttribute
    {
        readonly IReportApiVersions reporter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportApiVersionsAttribute"/> class.
        /// </summary>
        public ReportApiVersionsAttribute() => reporter = new DefaultApiVersionReporter();

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportApiVersionsAttribute"/> class.
        /// </summary>
        /// <param name="reportApiVersions">The <see cref="IReportApiVersions">object</see> used to report API versions.</param>
        public ReportApiVersionsAttribute( IReportApiVersions reportApiVersions )
        {
            Arg.NotNull( reportApiVersions, nameof( reportApiVersions ) );
            reporter = reportApiVersions;
        }

        /// <summary>
        /// Reports the discovered service API versions for the given context after an action has executed.
        /// </summary>
        /// <param name="context">The <see cref="ActionExecutedContext">context</see> for the executed action.</param>
        /// <remarks>This method will write the "api-supported-versions" and "api-deprecated-versions" HTTP headers into the
        /// response provided that there is a response and the executed action was not version-neutral.</remarks>
        public override void OnActionExecuted( ActionExecutedContext context )
        {
            var response = context.HttpContext.Response;

            if ( response == null )
            {
                return;
            }

            var model = context.ActionDescriptor.GetProperty<ApiVersionModel>();

            if ( model?.IsApiVersionNeutral == false )
            {
                reporter.Report( response.Headers, model );
            }
        }
    }
}