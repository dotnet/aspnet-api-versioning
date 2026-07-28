// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Asp.Versioning.OpenApi.Simulators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

public class GrpcWellKnownTypeSchemaTransformerTest
{
    [Fact]
    public async Task timestamp_should_be_described_as_date_time()
    {
        // arrange
        var schemas = await GenerateSchemasAsync();

        // act
        var schema = schemas["Timestamp"];

        // assert
        schema["type"].GetValue<string>().Should().Be( "string" );
        schema["format"].GetValue<string>().Should().Be( "date-time" );
        schema["properties"].Should().BeNull();
    }

    [Fact]
    public async Task duration_should_be_described_as_seconds_with_a_suffix()
    {
        // arrange
        var schemas = await GenerateSchemasAsync();

        // act
        var schema = schemas["Duration"];

        // assert
        schema["type"].GetValue<string>().Should().Be( "string" );
        schema["pattern"].GetValue<string>().Should().Be( @"^-?(?:0|[1-9]\d*)(?:\.\d{1,9})?s$" );
        schema["example"].GetValue<string>().Should().Be( "1.500s" );

        // the OpenAPI 'duration' format denotes an ISO 8601 duration, which is not what is serialized
        schema["format"].Should().BeNull();
    }

    [Fact]
    public async Task field_mask_should_be_described_as_comma_separated_paths()
    {
        // arrange
        var schemas = await GenerateSchemasAsync();

        // act
        var schema = schemas["FieldMask"];

        // assert
        schema["type"].GetValue<string>().Should().Be( "string" );
        schema["example"].GetValue<string>().Should().Be( "customer,lineItems" );
        schema["properties"].Should().BeNull();
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "customer" )]
    [InlineData( "customer,lineItems" )]
    [InlineData( "order.lineItems,customer" )]
    public async Task field_mask_pattern_should_match_serialized_form( string value )
    {
        // arrange
        var schemas = await GenerateSchemasAsync();
        var pattern = schemas["FieldMask"]["pattern"].GetValue<string>();

        // act
        var matched = Regex.IsMatch( value, pattern );

        // assert
        matched.Should().BeTrue();
    }

    [Theory]
    [InlineData( "line_items" )]
    [InlineData( "Customer" )]
    [InlineData( "customer," )]
    public async Task field_mask_pattern_should_not_match_other_forms( string value )
    {
        // arrange
        var schemas = await GenerateSchemasAsync();
        var pattern = schemas["FieldMask"]["pattern"].GetValue<string>();

        // act
        var matched = Regex.IsMatch( value, pattern );

        // assert
        matched.Should().BeFalse();
    }

    [Fact]
    public async Task any_should_be_described_as_an_open_object_with_a_type_url()
    {
        // arrange
        var schemas = await GenerateSchemasAsync();

        // act
        var schema = schemas["Any"];

        // assert
        schema["type"].GetValue<string>().Should().Be( "object" );
        schema["properties"]["@type"]["type"].GetValue<string>().Should().Be( "string" );

        // the message fields are not the serialized members
        schema["properties"]["typeUrl"].Should().BeNull();
        schema["properties"]["value"].Should().BeNull();
    }

    [Fact]
    public async Task unmapped_type_should_be_described_normally()
    {
        // arrange
        var schemas = await GenerateSchemasAsync();

        // act
        var schema = schemas["User"];

        // assert
        schema["type"].GetValue<string>().Should().Be( "object" );
        schema["properties"].Should().NotBeNull();
    }

    // the transformer is applied by ConfigureOpenApiOptions, so generating a document verifies that it is
    // both registered and correct
    private static async Task<JsonNode> GenerateSchemasAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddApiVersioning()
                        .AddApiExplorer( options => options.GroupNameFormat = "'v'VVV" )
                        .AddOpenApi();

        builder.Services.AddMvcCore().ConfigureApplicationPartManager( m => m.ApplicationParts.Clear() );

        await using var app = builder.Build();
        var api = app.NewVersionedApi( "Test" ).MapGroup( "/test" ).HasApiVersion( 1.0 );

        api.MapGet( "well-known", () => new WellKnownTypeModel() );
        api.MapGet( "user", () => new User() );
        app.MapOpenApi().WithDocumentPerVersion();

        var cancellationToken = TestContext.Current.CancellationToken;

        await app.StartAsync( cancellationToken );

        using var client = app.GetTestClient();
        var document = await client.GetFromJsonAsync<JsonNode>( "/openapi/v1.json", cancellationToken );

        await app.StopAsync( cancellationToken );

        return document["components"]["schemas"];
    }
}