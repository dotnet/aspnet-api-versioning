// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Web.Http;
using System.Web.Http.Filters;

/// <content>
/// Provides the implementation for ASP.NET Web API.
/// </content>
public sealed partial class ReportApiVersionsAttribute
{
    /// <summary>
    /// Occurs after the controller action has executed.
    /// </summary>
    /// <param name="actionExecutedContext">The <see cref="HttpActionExecutedContext">HTTP action context</see> that executed.</param>
    /// <remarks>This method will write the "api-supported-versions" and "api-deprecated-versions" HTTP headers into the
    /// response provided that there is a response and the executed action was not version-neutral.</remarks>
    public override void OnActionExecuted( HttpActionExecutedContext actionExecutedContext )
    {
        ArgumentNullException.ThrowIfNull( actionExecutedContext );

        var response = actionExecutedContext.Response;

        if ( response == null )
        {
            return;
        }

        var context = actionExecutedContext.ActionContext;
        var action = context.ActionDescriptor;
        var reporter = reportApiVersions ?? context.ControllerContext.Configuration.GetApiVersionReporter();
        var model = action.GetApiVersionMetadata().Map( reporter.Mapping );

        response.RequestMessage ??= actionExecutedContext.Request;
        reporter.Report( response, model );
    }
}