// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1822 // Mark members as static

namespace Asp.Versioning.Simulators;

using System.Web.Http;

[ApiVersion( "1.0" )]
[ApiVersion( "2.0" )]
[ApiVersion( "3.0" )]
[Route( "api/v{version:apiVersion}/test" )]
public sealed class ApiVersionedRouteController : ApiController
{
    public Task<string> Get() => Task.FromResult( "Test" );
}