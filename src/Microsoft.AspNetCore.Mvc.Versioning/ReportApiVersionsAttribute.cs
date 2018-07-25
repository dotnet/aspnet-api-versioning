﻿namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using Microsoft.AspNetCore.Mvc.Versioning;

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
        /// Reports the discovered service API versions for the given context before an action has executed.
        /// </summary>
        /// <param name="context">The <see cref="ActionExecutingContext">context</see> for the executing action.</param>
        /// <remarks>This method will write the "api-supported-versions" and "api-deprecated-versions" HTTP headers into the
        /// response provided the executing action is not version-neutral. This operation should be performed before the
        /// action is executed instead of after as HTTP headers cannot be specified after the response body has started
        /// streaming to the client.</remarks>
        public override void OnActionExecuting( ActionExecutingContext context )
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