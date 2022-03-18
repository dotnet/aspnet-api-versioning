// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnspecifiedApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Unspecified API Version";

    internal UnspecifiedApiVersionEndpoint( ILogger logger, string[]? displayNames = default )
        : base( c => OnExecute( c, displayNames, logger ), Empty, Name ) { }

    private static Task OnExecute( HttpContext context, string[]? candidateEndpoints, ILogger logger )
    {
        var services = context.RequestServices;
        var factory = services.GetRequiredService<IProblemDetailsFactory>();
        var (type, title) = ProblemDetailsDefaults.Unspecified;
        var problem = factory.CreateProblemDetails(
            context.Request,
            StatusCodes.Status400BadRequest,
            title,
            type,
            SR.ApiVersionUnspecified );

        if ( candidateEndpoints == null || candidateEndpoints.Length == 0 )
        {
            logger.ApiVersionUnspecified();
        }
        else
        {
            logger.ApiVersionUnspecifiedWithCandidates( candidateEndpoints );
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        return context.Response.WriteAsJsonAsync(
            problem,
            options: default,
            contentType: ProblemDetailsDefaults.MediaType.Json );
    }
}