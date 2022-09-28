// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using Microsoft.Owin.Testing;
#else
using Microsoft.AspNetCore.TestHost;
#endif

public abstract partial class HttpServerFixture : IDisposable
{
    private readonly Lazy<TestServer> server;
    private bool disposed;

    protected HttpServerFixture() => server = new( CreateServer );

    public TestServer Server => server.Value;

    public ICollection<Type> FilteredControllerTypes { get; } = new FilteredControllerTypes();

    public HttpClient CreateClient()
    {
#if NETFRAMEWORK
        var newClient = Server.HttpClient;
#else
        var newClient = Server.CreateClient();
#endif
        newClient.BaseAddress = new Uri( "http://localhost" );
        return newClient;
    }

    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected virtual void Dispose( bool disposing )
    {
        if ( disposed )
        {
            return;
        }

        disposed = true;

        if ( !disposing )
        {
            return;
        }

        if ( server.IsValueCreated )
        {
            server.Value.Dispose();
        }
    }

    protected virtual void OnAddApiVersioning( ApiVersioningOptions options ) { }
}