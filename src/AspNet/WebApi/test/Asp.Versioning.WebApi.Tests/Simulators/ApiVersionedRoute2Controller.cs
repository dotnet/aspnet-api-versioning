// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Simulators;

using System.Web.Http;

[ApiVersion( "5.0" )]
[ApiVersion( "4.0", Deprecated = true )]
[Route( "api/v{version:apiVersion}/test" )]
public sealed class ApiVersionedRoute2Controller : ApiController
{
    [MapToApiVersion( "4.0" )]
    public Task<string> GetV4() => Task.FromResult( "Test" );

    public Task<string> Get() => Task.FromResult( "Test" );
}