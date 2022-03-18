// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable CA1822 // Mark members as static

namespace Asp.Versioning.Simulators;

using System.Net.Http;
using System.Web.Http.Controllers;

[ApiVersionNeutral]
public sealed class NeutralController : IHttpController
{
    public Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken ) => throw new NotImplementedException();

    public Task<string> Get() => Task.FromResult( "Test" );
}