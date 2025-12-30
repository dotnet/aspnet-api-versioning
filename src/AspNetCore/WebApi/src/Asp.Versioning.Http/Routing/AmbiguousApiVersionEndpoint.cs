// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
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
        var apiVersions = context.ApiVersioningFeature().RawRequestedApiVersions;

#pragma warning disable CA1873
        logger.ApiVersionAmbiguous( [.. apiVersions] );
#pragma warning restore CA1873

        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        if ( !context.TryGetProblemDetailsService( out var problemDetails ) )
        {
            return Task.CompletedTask;
        }

        var detail = string.Format(
            CultureInfo.CurrentCulture,
            Format.MultipleDifferentApiVersionsRequested,
            string.Join( ", ", apiVersions ) );

        return problemDetails.TryWriteAsync(
            EndpointProblem.New(
                context,
                ProblemDetailsDefaults.Ambiguous,
                detail ) ).AsTask();
    }
}