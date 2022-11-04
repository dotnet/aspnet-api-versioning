// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class NotAcceptableEndpoint : Endpoint
{
    private const string Name = "406 HTTP Not Acceptable";

    internal NotAcceptableEndpoint() : base( OnExecute, Empty, Name ) { }

    private static Task OnExecute( HttpContext context ) =>
        EndpointProblem.UnsupportedApiVersion( context, StatusCodes.Status406NotAcceptable );
}