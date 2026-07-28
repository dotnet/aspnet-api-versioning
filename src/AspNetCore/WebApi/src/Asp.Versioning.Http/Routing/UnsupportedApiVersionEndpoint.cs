// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Routing;

internal static class UnsupportedApiVersionEndpoint
{
    private const string Name = " Unsupported API Version";

    internal static RouteEndpoint New( ApiVersioningOptions options ) =>
        ClientErrorEndpoint.New(
            context => EndpointProblem.UnsupportedApiVersion(
                context,
                options,
                options.UnsupportedApiVersionStatusCode ),
            options.UnsupportedApiVersionStatusCode + Name );
}