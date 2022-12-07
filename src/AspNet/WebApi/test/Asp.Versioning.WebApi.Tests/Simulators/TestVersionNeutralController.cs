// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Simulators;

using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

[ApiVersionNeutral]
[RoutePrefix( "api/neutral" )]
public sealed class TestVersionNeutralController : IHttpController
{
    public Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken ) => throw new NotImplementedException();

    [Route]
    public Task<string> Get() => Task.FromResult( "Test" );
}