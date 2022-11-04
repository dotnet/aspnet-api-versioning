// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class UnsupportedApiVersionEndpoint : Endpoint
{
    private const string Name = "400 Unsupported API Version";

    internal UnsupportedApiVersionEndpoint() : base( OnExecute, Empty, Name ) { }

    private static Task OnExecute( HttpContext context ) =>
        EndpointProblem.UnsupportedApiVersion( context, StatusCodes.Status400BadRequest );
}