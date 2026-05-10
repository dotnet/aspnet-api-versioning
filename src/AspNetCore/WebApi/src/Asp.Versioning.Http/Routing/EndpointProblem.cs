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

        if ( !string.IsNullOrEmpty( code ) )
        {
            newContext.ProblemDetails.Extensions[nameof( code )] = code;
        }

        return newContext;
    }

    internal static bool TryReportApiVersions( HttpContext context, ApiVersioningOptions options )
    {
        if ( options.ReportApiVersions &&
             context.Features.Get<ApiVersionPolicyFeature>() is ApiVersionPolicyFeature feature )
        {
            var reporter = context.RequestServices.GetRequiredService<IReportApiVersions>();
            var model = feature.Metadata.Map( reporter.Mapping );
            reporter.Report( context.Response, model );
            return true;
        }
        else
        {
            return false;
        }
    }

    internal static Task UnsupportedApiVersion(
        HttpContext context,
        ApiVersioningOptions options,
        int statusCode )
    {
        context.Response.StatusCode = statusCode;

        TryReportApiVersions( context, options );

        if ( context.RequestServices is not null &&
             context.TryGetProblemDetailsService( out var problemDetails ) )
        {
            var detail = string.Format(
                CultureInfo.CurrentCulture,
                Format.VersionedResourceNotSupported,
                new Uri( context.Request.GetDisplayUrl() ).SafePath,
                context.ApiVersioningFeature.RawRequestedApiVersion );

            return problemDetails.TryWriteAsync( New( context, Unsupported, detail ) ).AsTask();
        }

        return Task.CompletedTask;
    }

    internal static Task IntroducedInApiVersion(
        HttpContext context,
        ApiVersioningOptions options,
        int statusCode,
        ApiVersion introducedIn )
    {
        context.Response.StatusCode = statusCode;

        TryReportApiVersions( context, options );

        if ( context.RequestServices is not null &&
             context.TryGetProblemDetailsService( out var problemDetails ) )
        {
            var detail = string.Format(
                CultureInfo.CurrentCulture,
                Format.VersionedResourceNotIntroduced,
                new Uri( context.Request.GetDisplayUrl() ).SafePath,
                introducedIn,
                context.ApiVersioningFeature.RawRequestedApiVersion );

            return problemDetails.TryWriteAsync( New( context, Introduced, detail ) ).AsTask();
        }

        return Task.CompletedTask;
    }
}