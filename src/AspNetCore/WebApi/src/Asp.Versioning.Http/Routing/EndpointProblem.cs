// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using static Asp.Versioning.ProblemDetailsDefaults;

internal static class EndpointProblem
{
    internal static ProblemDetailsContext New( HttpContext context, ProblemDetailsInfo info, string detail )
    {
        var (type, title, code) = info;
        var newContext = new ProblemDetailsContext()
        {
            HttpContext = context,
            ProblemDetails =
            {
                Detail = detail,
                Status = context.Response.StatusCode,
                Title = title,
                Type = type,
            },
        };

        if ( string.IsNullOrEmpty( code ) )
        {
            newContext.ProblemDetails.Extensions[nameof( code )] = code;
        }

        return newContext;
    }

    internal static Task UnsupportedApiVersion(
        HttpContext context,
        ApiVersioningOptions options,
        int statusCode )
    {
        context.Response.StatusCode = statusCode;

        if ( options.ReportApiVersions &&
             context.Features.Get<ApiVersionPolicyFeature>() is ApiVersionPolicyFeature feature )
        {
            var reporter = context.RequestServices.GetRequiredService<IReportApiVersions>();
            var model = feature.Metadata.Map( reporter.Mapping );
            context.Response.OnStarting( ReportApiVersions, (reporter, context.Response, model) );
        }

        if ( context.TryGetProblemDetailsService( out var problemDetails ) )
        {
            var detail = string.Format(
                CultureInfo.CurrentCulture,
                Format.VersionedResourceNotSupported,
                new Uri( context.Request.GetDisplayUrl() ).SafeFullPath(),
                context.ApiVersioningFeature().RawRequestedApiVersion );

            return problemDetails.TryWriteAsync( New( context, Unsupported, detail ) ).AsTask();
        }

        return Task.CompletedTask;
    }

    private static Task ReportApiVersions( object state )
    {
        var (reporter, response, model) = ((IReportApiVersions, HttpResponse, ApiVersionModel)) state;
        reporter.Report( response, model );
        return Task.CompletedTask;
    }
}