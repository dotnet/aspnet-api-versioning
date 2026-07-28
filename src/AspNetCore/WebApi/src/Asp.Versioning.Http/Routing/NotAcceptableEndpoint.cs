// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

internal static class NotAcceptableEndpoint
{
    private const string Name = "406 HTTP Not Acceptable";

    internal static RouteEndpoint New( ApiVersioningOptions options ) =>
        ClientErrorEndpoint.New(
            context => EndpointProblem.UnsupportedApiVersion(
                context,
                options,
                StatusCodes.Status406NotAcceptable ),
            Name );
}