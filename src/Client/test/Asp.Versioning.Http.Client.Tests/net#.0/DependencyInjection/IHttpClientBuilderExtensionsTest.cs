// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Microsoft.Extensions.DependencyInjection;

using Asp.Versioning.Http;

public class IHttpClientBuilderExtensionsTest
{
    [Fact]
    public async Task add_api_version_should_decorate_http_client()
    {
        // arrange
        var services = new ServiceCollection();

        services.AddHttpClient( "Test" )
                .AddApiVersion( 1.0 )
                .AddHttpMessageHandler( () => new LastHandler() );

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient( "Test" );

        // act
        var response = await client.GetAsync( "http://tempuri.org" );

        // assert
        response.RequestMessage.RequestUri.Should().Be( new Uri( "http://tempuri.org?api-version=1.0" ) );
    }

    [Fact]
    public async Task add_api_version_should_use_registered_writer()
    {
        // arrange
        var services = new ServiceCollection();

        services.AddSingleton<IApiVersionWriter>( new HeaderApiVersionWriter( "x-ms-version" ) );
        services.AddHttpClient( "Test" )
                .AddApiVersion( 1, 0 )
                .AddHttpMessageHandler( () => new LastHandler() );

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient( "Test" );

        // act
        var response = await client.GetAsync( "http://tempuri.org" );

        // assert
        response.RequestMessage.Headers.GetValues( "x-ms-version" ).Single().Should().Be( "1.0" );
    }

    [Fact]
    public async Task add_api_version_should_ignore_registered_writer()
    {
        // arrange
        var writer = new QueryStringApiVersionWriter( "ver" );
        var services = new ServiceCollection();

        services.AddSingleton<IApiVersionWriter>( new HeaderApiVersionWriter( "x-ms-version" ) );
        services.AddHttpClient( "Test" )
                .AddApiVersion( 2022, 2, 1, default, writer )
                .AddHttpMessageHandler( () => new LastHandler() );

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IHttpClientFactory>();
        var client = factory.CreateClient( "Test" );

        // act
        var response = await client.GetAsync( "http://tempuri.org" );

        // assert
        response.RequestMessage.RequestUri.Should().Be( new Uri( "http://tempuri.org?ver=2022-02-01" ) );
    }

    [Fact]
    public void add_api_version_should_register_transient_header_enumerable()
    {
        // arrange
        var services = new ServiceCollection();

        services.AddHttpClient( "Test" ).AddApiVersion( 1.0 );

        var provider = services.BuildServiceProvider();

        // act
        var result1 = provider.GetRequiredService<ApiVersionHeaderEnumerable>();
        var result2 = provider.GetRequiredService<ApiVersionHeaderEnumerable>();

        // assert
        result1.Should().NotBeSameAs( result2 );
    }

    private sealed class LastHandler : DelegatingHandler
    {
        public HttpRequestMessage Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            var response = new HttpResponseMessage() { RequestMessage = request };
            return Task.FromResult( response );
        }
    }
}