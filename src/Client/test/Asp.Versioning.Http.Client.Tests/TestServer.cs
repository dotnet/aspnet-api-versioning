// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using System.Net;

internal sealed class TestServer : HttpMessageHandler
{
    private readonly HttpResponseMessage response;
    private bool disposed;

    public TestServer() => response = new( HttpStatusCode.OK );

    public TestServer( HttpResponseMessage response ) => this.response = response;

    protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken ) =>
        Task.FromResult( response );

    protected override void Dispose( bool disposing )
    {
        if ( disposed )
        {
            return;
        }

        base.Dispose( disposing );
        disposed = true;

        if ( disposing )
        {
            response.Dispose();
        }
    }
}