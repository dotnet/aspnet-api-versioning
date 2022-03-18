// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using System.Net;

internal sealed class TestServer : HttpMessageHandler
{
    private readonly HttpResponseMessage response;

    public TestServer() => response = new( HttpStatusCode.OK );

    public TestServer( HttpResponseMessage response ) => this.response = response;

    protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken ) =>
        Task.FromResult( response );
}