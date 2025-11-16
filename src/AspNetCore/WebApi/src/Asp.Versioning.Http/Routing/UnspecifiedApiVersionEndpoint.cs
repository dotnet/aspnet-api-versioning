// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnspecifiedApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Unspecified API Version";

    internal UnspecifiedApiVersionEndpoint(
        ILogger logger,
        ApiVersioningOptions options,
        string[]? displayNames = default )
        : base( context => OnExecute( context, options, displayNames, logger ), Empty, Name ) { }

    private static Task OnExecute(
        HttpContext context,
        ApiVersioningOptions options,
        string[]? candidateEndpoints,
        ILogger logger )
    {
        if ( candidateEndpoints == null || candidateEndpoints.Length == 0 )
        {
            logger.ApiVersionUnspecified();
        }
        else
        {
            logger.ApiVersionUnspecifiedWithCandidates( candidateEndpoints );
        }

        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        EndpointProblem.TryReportApiVersions( context, options );

        if ( context.TryGetProblemDetailsService( out var problemDetails ) )
        {
            return problemDetails.TryWriteAsync(
                EndpointProblem.New(
                    context,
                    ProblemDetailsDefaults.Unspecified,
                    detail: SR.ApiVersionUnspecified ) ).AsTask();
        }

        return Task.CompletedTask;
    }
}