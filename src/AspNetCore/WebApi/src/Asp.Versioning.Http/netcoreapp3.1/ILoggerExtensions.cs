// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.Extensions.Logging;
using static Microsoft.Extensions.Logging.LoggerMessage;
using static Microsoft.Extensions.Logging.LogLevel;

internal static class ILoggerExtensions
{
    private static readonly Action<ILogger, string?, Exception?> apiVersionInvalid =
        Define<string?>( Information, 1, "Request contained the service API version '{ApiVersion}', which is not valid" );

    private static readonly Action<ILogger, string[]?, Exception?> apiVersionAmbiguous =
        Define<string[]?>( Information, 2, "The requested API version is ambiguous. Requested API Versions: {ApiVersions}" );

    private static readonly Action<ILogger, Exception?> apiVersionUnspecified =
        Define( Information, 3, "Request did not specify an API version" );

    private static readonly Action<ILogger, string[], Exception?> apiVersionUnspecifiedWithCandidates =
        Define<string[]>( Information, 4, "Request did not specify an API version, but multiple candidate endpoints were found. Candidate endpoints: {CandidateEndpoints}" );

    internal static void ApiVersionInvalid( this ILogger logger, string? apiVersion ) => apiVersionInvalid( logger, apiVersion, null );

    internal static void ApiVersionAmbiguous( this ILogger logger, string[]? apiVersions ) => apiVersionAmbiguous( logger, apiVersions, null );

    internal static void ApiVersionUnspecified( this ILogger logger ) => apiVersionUnspecified( logger, null );

    internal static void ApiVersionUnspecifiedWithCandidates( this ILogger logger, string[] candidateEndpoints ) =>
        apiVersionUnspecifiedWithCandidates( logger, candidateEndpoints, null );
}