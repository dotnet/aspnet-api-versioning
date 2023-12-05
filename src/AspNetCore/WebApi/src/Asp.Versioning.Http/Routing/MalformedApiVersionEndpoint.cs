// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
        var requestedVersion = context.ApiVersioningFeature().RawRequestedApiVersion;

        logger.ApiVersionInvalid( requestedVersion );
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        if ( !context.TryGetProblemDetailsService( out var problemDetails ) )
        {
            return Task.CompletedTask;
        }

        var detail = string.Format(
            CultureInfo.CurrentCulture,
            Format.VersionedResourceNotSupported,
            new Uri( context.Request.GetDisplayUrl() ).SafeFullPath(),
            requestedVersion );

        return problemDetails.TryWriteAsync(
            EndpointProblem.New(
                context,
                ProblemDetailsDefaults.Invalid,
                detail ) ).AsTask();
    }
}