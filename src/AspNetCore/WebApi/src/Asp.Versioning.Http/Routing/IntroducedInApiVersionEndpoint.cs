// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Routing;

using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.EndpointMetadataCollection;

internal sealed class IntroducedInApiVersionEndpoint : Endpoint
{
    private const string Name = " Introduced API Version";

    internal IntroducedInApiVersionEndpoint( int statusCode )
        : base(
            context =>
            {
                context.Response.StatusCode = statusCode;
                return Task.CompletedTask;
            },
            Empty,
            statusCode + Name )
    { }
}