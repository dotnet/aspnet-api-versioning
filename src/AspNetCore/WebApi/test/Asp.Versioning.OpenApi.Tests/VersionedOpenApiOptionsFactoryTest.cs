// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi;

using Asp.Versioning.OpenApi.Simulators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

public class VersionedOpenApiOptionsFactoryTest
{
    // the options of a document are created the first time it is requested. creating the options of different
    // documents at the same time used to corrupt each other, which depends on timing, so the host is started again
    // and every document is requested at the same moment several times
    private const int Attempts = 50;

    [Fact]
    public async Task open_api_documents_requested_concurrently_should_all_be_configured()
    {
        // arrange
        var cancellationToken = TestContext.Current.CancellationToken;

        for ( var attempt = 0; attempt < Attempts; attempt++ )
        {
            var builder = WebApplication.CreateBuilder();

            builder.WebHost.UseTestServer();
            builder.Services.AddApiVersioning()
                            .AddApiExplorer( options => options.GroupNameFormat = "'v'VVV" )
                            .AddOpenApi();

            IsolateMinimalApis( builder.Services );

            await using var app = builder.Build();

            app.NewVersionedApi( "One" ).MapGroup( "/one" ).HasApiVersion( 1.0 ).MapGet( "{id:int}", MinimalApi.Get );
            app.NewVersionedApi( "Two" ).MapGroup( "/two" ).HasApiVersion( 2.0 ).MapGet( "{id:int}", MinimalApi.Get );
            app.MapOpenApi().WithDocumentPerVersion();

            await app.StartAsync( cancellationToken );

            // act
            var documents = await GetDocumentsAsync( app, cancellationToken );

            // assert
            AssertConfigured( documents[0], "1.0", "/one/{id}" );
            AssertConfigured( documents[1], "2.0", "/two/{id}" );
        }
    }

    private static async Task<JsonNode[]> GetDocumentsAsync( WebApplication app, CancellationToken cancellationToken )
    {
        using var client = app.GetTestClient();
        var start = new TaskCompletionSource();

        async Task<JsonNode> GetAsync( string documentName )
        {
            await start.Task;
            return await client.GetFromJsonAsync<JsonNode>( $"/openapi/{documentName}.json", cancellationToken );
        }

        var requests = Task.WhenAll( GetAsync( "v1" ), GetAsync( "v2" ) );

        // release both requests together
        start.SetResult();

        return await requests;
    }

    private static void AssertConfigured( JsonNode document, string version, string path )
    {
        // a document that was created with the default options has neither the api version nor any of the api paths
        document.Should().NotBeNull();
        document["info"]["version"].GetValue<string>().Should().Be( version );
        document["paths"].AsObject().ContainsKey( path ).Should().BeTrue();
    }

    private static void IsolateMinimalApis( IServiceCollection services ) =>
        services.AddMvcCore().ConfigureApplicationPartManager( m => m.ApplicationParts.Clear() );
}