// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnsupportedMediaTypeEndpoint : Endpoint
{
    private const string Name = "415 HTTP Unsupported Media Type";

    internal UnsupportedMediaTypeEndpoint( ApiVersioningOptions options )
        : base(
            context => EndpointProblem.UnsupportedApiVersion(
                context,
                options,
                StatusCodes.Status415UnsupportedMediaType ),
            Empty,
            Name ) { }
}