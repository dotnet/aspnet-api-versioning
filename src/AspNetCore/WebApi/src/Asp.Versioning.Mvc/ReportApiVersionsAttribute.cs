// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

/// <content>
/// Provides additional implementation specific to ASP.NET Core.
/// </content>
[CLSCompliant( false )]
public sealed partial class ReportApiVersionsAttribute
{
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
        ArgumentNullException.ThrowIfNull( context );

        var httpContext = context.HttpContext;
        var endpoint = httpContext.GetEndpoint();

        if ( endpoint == null )
        {
            return;
        }

        var metadata = endpoint.Metadata.GetMetadata<ApiVersionMetadata>();

        if ( metadata == null )
        {
            return;
        }

        var reporter = reportApiVersions ?? httpContext.RequestServices.GetRequiredService<IReportApiVersions>();
        var model = metadata.Map( reporter.Mapping );
        var response = httpContext.Response;

        response.OnStarting( ReportApiVersions, (reporter, response, model) );
    }

    private static Task ReportApiVersions( object state )
    {
        var (reporter, response, model) = ((IReportApiVersions, HttpResponse, ApiVersionModel)) state;
        reporter.Report( response, model );
        return Task.CompletedTask;
    }
}