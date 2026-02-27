// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Asp.Versioning.OpenApi.Simulators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

public class AcceptanceTest
{
    /// <summary>
    /// Verifies that minimal API endpoints produce a non-empty OpenAPI document.
    /// <c>AddApiExplorer</c> internally calls <c>AddMvcCore().AddApiExplorer()</c>,
    /// which auto-discovers controllers from the test assembly. Application parts
    /// are cleared to isolate the test to minimal API endpoints only.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task minimal_api_should_generate_expected_open_api_document()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddProblemDetails();
        builder.Services.AddApiVersioning( options => AddPolicies( options ) )
                        .AddApiExplorer( options => options.GroupNameFormat = "'v'VVV" )
                        .AddOpenApi();
        builder.Services.AddMvcCore()
                        .ConfigureApplicationPartManager(
                            m => m.ApplicationParts.Clear() );

        var app = builder.Build();
        var api = app.NewVersionedApi( "Test" )
                     .MapGroup( "/test" )
                     .HasApiVersion( 1.0 );

        api.MapGet( "{id:int}", MinimalApi.Get ).Produces<int>().Produces( 400 );
        app.MapOpenApi().WithDocumentPerVersion();

        var cancellationToken = TestContext.Current.CancellationToken;
        await app.StartAsync( cancellationToken );

        using var client = app.GetTestClient();

        // act
        var actual = await client.GetFromJsonAsync<JsonNode>( "/openapi/v1.json", cancellationToken );

        // assert
        actual!["info"]!["version"]!.GetValue<string>().Should().Be( "1.0" );

        var paths = actual["paths"]!.AsObject();

        paths.Select( p => p.Key ).Should().Contain( "/test/{id}" );
        paths.Select( p => p.Key ).Should().NotContain( "/Test" );

        var operation = paths["/test/{id}"]!["get"]!;
        var parameters = operation["parameters"]!.AsArray();

        parameters.Should().Contain( p => p!["name"]!.GetValue<string>() == "id" );
        parameters.Should().Contain( p => p!["name"]!.GetValue<string>() == "api-version" );
    }

    [Fact]
    public async Task controller_should_generate_expected_open_api_document()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddControllers()
                        .AddApplicationPart( GetType().Assembly );
        builder.Services.AddApiVersioning( options => AddPolicies( options ) )
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

    [Fact]
    public async Task mixed_api_should_generate_expected_open_api_document()
    {
        // arrange
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddControllers()
                        .AddApplicationPart( GetType().Assembly );
        builder.Services.AddApiVersioning( options => AddPolicies( options ) )
                        .AddMvc()
                        .AddApiExplorer( options => options.GroupNameFormat = "'v'VVV" )
                        .AddOpenApi();

        var app = builder.Build();

        app.MapControllers();
        var api = app.NewVersionedApi( "Test" )
                     .MapGroup( "/minimal" )
                     .HasApiVersion( 1.0 );

        api.MapGet( "{id:int}", MinimalApi.Get ).Produces<int>().Produces( 400 );
        app.MapOpenApi().WithDocumentPerVersion();

        var cancellationToken = TestContext.Current.CancellationToken;
        await app.StartAsync( cancellationToken );

        using var client = app.GetTestClient();

        // act
        var actual = await client.GetFromJsonAsync<JsonNode>( "/openapi/v1.json", cancellationToken );

        // assert
        actual!["info"]!["version"]!.GetValue<string>().Should().Be( "1.0" );

        var paths = actual["paths"]!.AsObject();

        paths.Select( p => p.Key ).Should().Contain( "/minimal/{id}" );
        paths.Select( p => p.Key ).Should().Contain( "/Test" );
    }

    private static ApiVersioningOptions AddPolicies( ApiVersioningOptions options )
    {
        options.Policies.Deprecate( 1.0 )
                        .Effective( 2026, 1, 1 )
                        .Link( "http://my.api.com/policies/versions/deprecated.html" )
                            .Title( "Version Deprecation Policy" )
                            .Type( "text/html" );

        options.Policies.Sunset( 1.0 )
                        .Effective( 2026, 2, 10 )
                        .Link( "http://my.api.com/policies/versions/sunset.html" )
                            .Title( "Version Sunset Policy" )
                            .Type( "text/html" );

        return options;
    }
}