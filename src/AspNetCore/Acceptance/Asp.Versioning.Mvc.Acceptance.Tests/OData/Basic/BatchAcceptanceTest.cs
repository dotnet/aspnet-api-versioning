// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Basic;

using Asp.Versioning;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using static System.Net.Http.HttpMethod;

[Collection( "OData" + nameof( BasicTestCollection ) )]
public abstract class BatchAcceptanceTest : AcceptanceTest
{
    protected BatchAcceptanceTest( ODataFixture fixture ) : base( fixture ) { }

    protected HttpRequestMessage NewBatch(
        string requestUri,
        HttpContent request,
        params HttpContent[] otherRequests )
    {
        var content = new MultipartContent( "mixed" ) { request };

        for ( var i = 0; i < otherRequests.Length; i++ )
        {
            content.Add( otherRequests[i] );
        }

        return new( Post, new Uri( Client.BaseAddress, requestUri ) ) { Content = content };
    }

    protected HttpMessageContent NewGet( string requestUri ) => NewRequest<object>( Get, requestUri );

    protected HttpMessageContent NewDelete( string requestUri ) => NewRequest<object>( Delete, requestUri );

    protected HttpMessageContent NewPut<T>( string requestUri, T entity )
        where T : class => NewRequest( Put, requestUri, entity );

    protected HttpMessageContent NewRequest<T>( HttpMethod method, string requestUri, T entity = default )
        where T : class
    {
        var request = new HttpRequestMessage( method, new Uri( Client.BaseAddress, requestUri ) );

        if ( !Equals( entity, default( T ) ) )
        {
            request.Content = new ObjectContent<T>(
                    entity,
                    new JsonMediaTypeFormatter(),
                    JsonMediaTypeFormatter.DefaultMediaType );
        }

        var content = new HttpMessageContent( request )
        {
            Headers =
            {
                ContentType = MediaTypeHeaderValue.Parse( "application/http" ),
            },
        };

        content.Headers.TryAddWithoutValidation( "Content-Transfer-Encoding", "binary" );

        return content;
    }

    protected static async Task<T> ReadAsAsync<T>( HttpContent content, T example )
    {
        content.Headers.ContentType.Parameters.Add( new( "msgtype", "response" ) );
        using var response = await content.ReadAsHttpResponseMessageAsync();
        return await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );
    }

    protected static async IAsyncEnumerable<HttpResponseMessage> ReadAsResponses( HttpResponseMessage response )
    {
        var multipart = await response.Content.ReadAsMultipartAsync();
        var contents = multipart.Contents;

        for ( var i = 0; i < contents.Count; i++ )
        {
            var content = contents[i];
            content.Headers.ContentType.Parameters.Add( new( "msgtype", "response" ) );
            yield return await content.ReadAsHttpResponseMessageAsync();
        }
    }
}