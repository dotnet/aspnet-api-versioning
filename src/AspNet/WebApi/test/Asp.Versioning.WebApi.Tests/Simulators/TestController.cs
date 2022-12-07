// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Simulators;

using System.Net.Http;
using System.Web.Http.Controllers;

public sealed class TestController : IHttpController
{
    public Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken ) => throw new NotImplementedException();

    public Task<string> Get() => Task.FromResult( "Test" );
}