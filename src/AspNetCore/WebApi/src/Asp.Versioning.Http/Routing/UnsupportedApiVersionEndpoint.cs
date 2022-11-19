// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnsupportedApiVersionEndpoint : Endpoint
{
    private const string Name = " Unsupported API Version";

    internal UnsupportedApiVersionEndpoint( ApiVersioningOptions options )
        : base(
            context => EndpointProblem.UnsupportedApiVersion(
                context,
                options,
                options.UnsupportedApiVersionStatusCode ),
            Empty,
            options.UnsupportedApiVersionStatusCode + Name )
    { }
}