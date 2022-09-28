// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using static System.Net.Http.HttpMethod;

[Trait( "Kind", "Acceptance" )]
#if NETFRAMEWORK
[Trait( "ASP.NET", "Web API" )]
#else
[Trait( "ASP.NET", "Core" )]
#endif
public abstract partial class AcceptanceTest : IDisposable
{
    private static readonly HttpMethod Patch = new( "PATCH" );
    private readonly Lazy<HttpClient> client;
    private bool disposed;

    protected AcceptanceTest( HttpServerFixture fixture ) => client = new( fixture.CreateClient );

    protected HttpClient Client => client.Value;

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

        if ( disposing && client.IsValueCreated )
        {
            client.Value.Dispose();
        }
    }

    private HttpRequestMessage CreateRequest<TEntity>( string requestUri, TEntity entity, HttpMethod method )
    {
        AddDefaultAcceptHeaderIfNecessary();

        var request = new HttpRequestMessage( method, requestUri );

        if ( Equals( entity, default( TEntity ) ) )
        {
            return request;
        }

        if ( entity is HttpContent content )
        {
            request.Content = content;
        }
        else
        {
            request.Content = new ObjectContent<TEntity>(
                entity,
                new JsonMediaTypeFormatter(),
                JsonMediaTypeFormatter.DefaultMediaType );
        }

        return request;
    }

    private HttpRequestMessage CreateRequest( string requestUri, HttpContent content, HttpMethod method )
    {
        AddDefaultAcceptHeaderIfNecessary();
        return new HttpRequestMessage( method, requestUri ) { Content = content };
    }

    protected virtual Task<HttpResponseMessage> GetAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Get ) );

    protected virtual Task<HttpResponseMessage> PostAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Post ) );

    protected virtual Task<HttpResponseMessage> PostAsync( string requestUri, HttpContent content ) => Client.SendAsync( CreateRequest( requestUri, content, Post ) );

    protected virtual Task<HttpResponseMessage> PutAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Put ) );

    protected virtual Task<HttpResponseMessage> PutAsync( string requestUri, HttpContent content ) => Client.SendAsync( CreateRequest( requestUri, content, Put ) );

    protected virtual Task<HttpResponseMessage> PatchAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Patch ) );

    protected virtual Task<HttpResponseMessage> PatchAsync( string requestUri, HttpContent content ) => Client.SendAsync( CreateRequest( requestUri, content, Patch ) );

    protected virtual Task<HttpResponseMessage> DeleteAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Delete ) );

    private void AddDefaultAcceptHeaderIfNecessary()
    {
        var accept = Client.DefaultRequestHeaders.Accept;
        var mediaType = JsonMediaTypeFormatter.DefaultMediaType.MediaType;
        var comparer = StringComparer.OrdinalIgnoreCase;

        foreach ( var item in accept )
        {
            if ( comparer.Equals( item.MediaType, mediaType ) )
            {
                return;
            }
        }

        accept.Add( new MediaTypeWithQualityHeaderValue( mediaType ) );
    }
}