// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.LogLevel;

internal static partial class ILoggerExtensions
{
    [LoggerMessage( EventId = 1, Level = Information, Message = "Request contained the API version '{apiVersion}', which is not valid" )]
    internal static partial void ApiVersionInvalid( this ILogger logger, string? apiVersion );

    [LoggerMessage( EventId = 2, Level = Information, Message = "The requested API version is ambiguous. Requested API Versions: {apiVersions}" )]
    internal static partial void ApiVersionAmbiguous( this ILogger logger, string[]? apiVersions );

    [LoggerMessage( EventId = 3, Level = Information, Message = "Request did not specify an API version" )]
    internal static partial void ApiVersionUnspecified( this ILogger logger );

    [LoggerMessage( EventId = 4, Level = Information, Message = "Request did not specify an API version, but multiple candidate endpoints were found. Candidate endpoints: {candidateEndpoints}" )]
    internal static partial void ApiVersionUnspecifiedWithCandidates( this ILogger logger, string[] candidateEndpoints );
}