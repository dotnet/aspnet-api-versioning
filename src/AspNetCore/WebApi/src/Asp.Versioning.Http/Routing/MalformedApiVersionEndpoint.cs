// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class MalformedApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Invalid API Version";

    internal MalformedApiVersionEndpoint( ILogger logger )
        : base( c => OnExecute( c, logger ), Empty, Name ) { }

    private static Task OnExecute( HttpContext context, ILogger logger )
    {
        var services = context.RequestServices;
        var factory = services.GetRequiredService<IProblemDetailsFactory>();
        var requestUrl = new Uri( context.Request.GetDisplayUrl() ).SafeFullPath();
        var requestedVersion = context.ApiVersioningFeature().RawRequestedApiVersion;
        var (type, title) = ProblemDetailsDefaults.Invalid;
        var detail = string.Format(
            CultureInfo.CurrentCulture,
            SR.VersionedResourceNotSupported,
            requestUrl,
            requestedVersion );
        var problem = factory.CreateProblemDetails(
            context.Request,
            StatusCodes.Status400BadRequest,
            title,
            type,
            detail );

        logger.ApiVersionInvalid( requestedVersion );
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        return context.Response.WriteAsJsonAsync(
            problem,
            options: default,
            contentType: ProblemDetailsDefaults.MediaType.Json );
    }
}