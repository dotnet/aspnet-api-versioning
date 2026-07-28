// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

internal static class UnspecifiedApiVersionEndpoint
{
    private const string Name = "400 Unspecified API Version";

    internal static RouteEndpoint New(
        ILogger logger,
        ApiVersioningOptions options,
        string[]? displayNames = default ) =>
        ClientErrorEndpoint.New( context => OnExecute( context, options, displayNames, logger ), Name );

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