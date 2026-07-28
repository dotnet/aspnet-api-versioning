// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

internal static class UnsupportedMediaTypeEndpoint
{
    private const string Name = "415 HTTP Unsupported Media Type";

    internal static RouteEndpoint New( ApiVersioningOptions options ) =>
        ClientErrorEndpoint.New(
            context => EndpointProblem.UnsupportedApiVersion(
                context,
                options,
                StatusCodes.Status415UnsupportedMediaType ),
            Name );
}