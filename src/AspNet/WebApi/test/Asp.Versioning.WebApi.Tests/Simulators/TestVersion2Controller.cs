// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Simulators;

using System.Net.Http;
using System.Web.Http.Controllers;

[ControllerName( "Test" )]
[ApiVersion( "2.0" )]
[ApiVersion( "3.0" )]
[ApiVersion( "1.8", Deprecated = true )]
[ApiVersion( "1.9", Deprecated = true )]
public sealed class TestVersion2Controller : IHttpController
{
    public Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken ) => throw new NotImplementedException();

    public Task<string> Get() => Task.FromResult( "Test" );

    [MapToApiVersion( "3.0" )]
    public Task<string> Get3() => Task.FromResult( "Test" );
}