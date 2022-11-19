// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

internal static class EndpointProblem
{
    internal static Task UnsupportedApiVersion(
        HttpContext context,
        ApiVersioningOptions options,
        int statusCode )
    {
        var services = context.RequestServices;
        var factory = services.GetRequiredService<IProblemDetailsFactory>();
        var url = new Uri( context.Request.GetDisplayUrl() ).SafeFullPath();
        var apiVersion = context.ApiVersioningFeature().RawRequestedApiVersion;
        var (type, title) = ProblemDetailsDefaults.Unsupported;
        var detail = string.Format(
            CultureInfo.CurrentCulture,
            SR.VersionedResourceNotSupported,
            url,
            apiVersion );
        var problem = factory.CreateProblemDetails(
            context.Request,
            statusCode,
            title,
            type,
            detail );

        context.Response.StatusCode = statusCode;

        if ( options.ReportApiVersions &&
             context.Features.Get<ApiVersionPolicyFeature>() is ApiVersionPolicyFeature feature )
        {
            var reporter = services.GetRequiredService<IReportApiVersions>();
            var model = feature.Metadata.Map( reporter.Mapping );
            context.Response.OnStarting( ReportApiVersions, (reporter, context.Response, model) );
        }

        return context.Response.WriteAsJsonAsync(
            problem,
            options: default,
            contentType: ProblemDetailsDefaults.MediaType.Json );
    }

    private static Task ReportApiVersions( object state )
    {
        var (reporter, response, model) = ((IReportApiVersions, HttpResponse, ApiVersionModel)) state;
        reporter.Report( response, model );
        return Task.CompletedTask;
    }
}