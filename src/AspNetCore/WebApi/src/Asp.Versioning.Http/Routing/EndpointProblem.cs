// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

internal static class EndpointProblem
{
    internal static Task UnsupportedApiVersion( HttpContext context, int statusCode )
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

        return context.Response.WriteAsJsonAsync(
            problem,
            options: default,
            contentType: ProblemDetailsDefaults.MediaType.Json );
    }
}