// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class AmbiguousApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Ambiguous API Version";

    internal AmbiguousApiVersionEndpoint( ILogger logger )
        : base( c => OnExecute( c, logger ), Empty, Name ) { }

    private static Task OnExecute( HttpContext context, ILogger logger )
    {
        var services = context.RequestServices;
        var factory = services.GetRequiredService<IProblemDetailsFactory>();
        var apiVersions = context.ApiVersioningFeature().RawRequestedApiVersions;
        var (type, title) = ProblemDetailsDefaults.Ambiguous;
        var detail = string.Format(
            CultureInfo.CurrentCulture,
            CommonSR.MultipleDifferentApiVersionsRequested,
            string.Join( ", ", apiVersions ) );
        var problem = factory.CreateProblemDetails(
            context.Request,
            StatusCodes.Status400BadRequest,
            title,
            type,
            detail );

        logger.ApiVersionAmbiguous( apiVersions.ToArray() );
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        return context.Response.WriteAsJsonAsync(
            problem,
            options: default,
            contentType: ProblemDetailsDefaults.MediaType.Json );
    }
}