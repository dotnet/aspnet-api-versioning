// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnsupportedApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Unsupported API Version";

    internal UnsupportedApiVersionEndpoint() : base( OnExecute, Empty, Name ) { }

    private static Task OnExecute( HttpContext context )
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
            StatusCodes.Status400BadRequest,
            title,
            type,
            detail );

        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        return context.Response.WriteAsJsonAsync(
            problem,
            options: default,
            contentType: ProblemDetailsDefaults.MediaType.Json );
    }
}