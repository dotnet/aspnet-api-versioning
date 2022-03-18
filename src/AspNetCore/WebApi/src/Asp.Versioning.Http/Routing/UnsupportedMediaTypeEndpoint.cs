// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnsupportedMediaTypeEndpoint : Endpoint
{
    private const string Name = "415 HTTP Unsupported Media Type";

    internal UnsupportedMediaTypeEndpoint() : base( OnExecute, Empty, Name ) { }

    private static Task OnExecute( HttpContext context )
    {
        context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
        return Task.CompletedTask;
    }
}