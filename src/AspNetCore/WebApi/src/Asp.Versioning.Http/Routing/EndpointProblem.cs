// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;
using static Asp.Versioning.ProblemDetailsDefaults;

internal static class EndpointProblem
{
    internal static ProblemDetailsContext New( HttpContext context, ProblemDetailsInfo info, string detail )
    {
        const string Code = nameof( Code );
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
            newContext.ProblemDetails.Extensions[Code] = code;
        }

        return newContext;
    }

    internal static Task UnsupportedApiVersion( HttpContext context, int statusCode )
    {
        context.Response.StatusCode = statusCode;

        if ( context.TryGetProblemDetailsService( out var problemDetails ) )
        {
            var detail = string.Format(
                CultureInfo.CurrentCulture,
                SR.VersionedResourceNotSupported,
                new Uri( context.Request.GetDisplayUrl() ).SafeFullPath(),
                context.ApiVersioningFeature().RawRequestedApiVersion );

            return problemDetails.WriteAsync( New( context, Unsupported, detail ) ).AsTask();
        }

        return Task.CompletedTask;
    }
}