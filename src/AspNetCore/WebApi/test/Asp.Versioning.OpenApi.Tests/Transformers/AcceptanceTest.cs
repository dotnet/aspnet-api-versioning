// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Asp.Versioning.OpenApi.Simulators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

public class AcceptanceTest
{
    [Fact]
    public async Task minimal_api_should_generate_expected_open_api_document()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddControllers()
                        .AddApplicationPart( GetType().Assembly );
        builder.Services.AddApiVersioning( options => AddSunsetPolicy( options ) )
                        .AddMvc()
                        .AddApiExplorer( options => options.GroupNameFormat = "'v'VVV" )
                        .AddOpenApi();

        var app = builder.Build();
        var api = app.NewVersionedApi( "Test" )
                     .MapGroup( "/test" )
                     .HasApiVersion( 1.0 );

        api.MapGet( "{id:int}", MinimalApi.Get ).Produces<int>().Produces( 400 );
        app.MapOpenApi().WithDocumentPerVersion();

        var cancellationToken = TestContext.Current.CancellationToken;
        using var stream = File.OpenRead( Path.Combine( AppContext.BaseDirectory, "Content", "v1.json" ) );
        var expected = await JsonNode.ParseAsync( stream, default, default, cancellationToken );

        await app.StartAsync( cancellationToken );

        using var client = app.GetTestClient();

        // act
        var actual = await client.GetFromJsonAsync<JsonNode>( "/openapi/v1.json", cancellationToken );

        // assert
        JsonNode.DeepEquals( actual, expected ).Should().BeTrue();
    }

    [Fact]
    public async Task controller_should_generate_expected_open_api_document()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddControllers()
                        .AddApplicationPart( GetType().Assembly );
        builder.Services.AddApiVersioning( options => AddSunsetPolicy( options ) )
                        .AddMvc()
                        .AddApiExplorer( options => options.GroupNameFormat = "'v'VVV" )
                        .AddOpenApi();

        var app = builder.Build();

        app.MapControllers();
        app.MapOpenApi().WithDocumentPerVersion();

        var cancellationToken = TestContext.Current.CancellationToken;
        using var stream = File.OpenRead( Path.Combine( AppContext.BaseDirectory, "Content", "v1.json" ) );
        var expected = await JsonNode.ParseAsync( stream, default, default, cancellationToken );

        await app.StartAsync( cancellationToken );

        using var client = app.GetTestClient();

        // act
        var actual = await client.GetFromJsonAsync<JsonNode>( "/openapi/v1.json", cancellationToken );

        // assert
        JsonNode.DeepEquals( actual, expected ).Should().BeTrue();
    }

    private static ApiVersioningOptions AddSunsetPolicy( ApiVersioningOptions options )
    {
        options.Policies.Sunset( 1.0 )
                        .Effective( 2026, 2, 10 )
                        .Link( "policy.html" )
                            .Title( "Versioning Policy" )
                            .Type( "text/html" );

        return options;
    }
}