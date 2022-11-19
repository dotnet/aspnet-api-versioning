// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class NotAcceptableEndpoint : Endpoint
{
    private const string Name = "406 HTTP Not Acceptable";

    internal NotAcceptableEndpoint( ApiVersioningOptions options )
        : base(
            context => EndpointProblem.UnsupportedApiVersion(
                context,
                options,
                StatusCodes.Status406NotAcceptable ),
            Empty,
            Name ) { }
}