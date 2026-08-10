// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OpenApi.Transformers;

using Asp.Versioning.OpenApi.Simulators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

public class XmlCommentsStructureTest
{
    [Fact]
    public async Task returns_should_describe_the_only_response()
    {
        // arrange
        var paths = await GeneratePathsAsync();

        // act
        var description = paths["/test/one"]["get"]["responses"]["200"]["description"];

        // assert
        description.GetValue<string>().Should().Be( "The only answer." );
    }

    [Fact]
    public async Task returns_should_not_describe_multiple_responses()
    {
        // arrange
        var paths = await GeneratePathsAsync();

        // act
        var responses = paths["/test/many"]["get"]["responses"];

        // assert
        responses["200"]["description"].GetValue<string>().Should().NotBe( "An ambiguous answer." );
        responses["400"]["description"].GetValue<string>().Should().NotBe( "An ambiguous answer." );
    }

    [Fact]
    public async Task response_should_take_precedence_over_returns()
    {
        // arrange
        var paths = await GeneratePathsAsync();

        // act
        var responses = paths["/test/{id}"]["get"]["responses"];

        // assert
        responses["200"]["description"].GetValue<string>().Should().Be( "Pass" );
        responses["400"]["description"].GetValue<string>().Should().Be( "Fail" );
    }

    [Fact]
    public void value_should_be_retrieved_for_property()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Status ) );

        // act
        var value = comments.GetValue( property );

        // assert
        value.Should().Be( "Defaults to `Pending`." );
    }

    [Fact]
    public void value_should_be_retrieved_when_summary_is_absent()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Kind ) );

        // act
        var value = comments.GetValue( property );

        // assert
        value.Should().Be( "The unique identifier." );
        comments.GetSummary( property ).Should().BeEmpty();
    }

    [Fact]
    public void value_should_be_empty_when_undocumented()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Tags ) );

        // act
        var value = comments.GetValue( property );

        // assert
        value.Should().BeEmpty();
    }

    [Fact]
    public void bulleted_list_should_be_resolved_into_bullets()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Tags ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be( "Gets or sets the tags.\n* First\n* Second" );
    }

    [Fact]
    public void numbered_list_should_be_resolved_into_bullets()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Steps ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be( "Gets or sets the steps.\n1. First\n2. Second" );
    }

    [Fact]
    public void list_item_should_be_resolved_from_a_term_and_description()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Definitions ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be(
            "Gets or sets the definitions.\n" +
            "* **Foo**: Does foo\n" +
            "* Bar\n" +
            "* Just a description\n" +
            "* Plain text" );
    }

    [Fact]
    public void table_should_be_resolved_into_a_markdown_table()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Matrix ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be(
            "Gets or sets the matrix.\n" +
            "\n" +
            "| Name | Meaning |\n" +
            "| --- | --- |\n" +
            "| A | First |\n" +
            "| B | Second |" );
    }

    [Fact]
    public void table_without_a_header_should_be_resolved_with_a_blank_one()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Headerless ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be(
            "Gets or sets the headerless.\n" +
            "\n" +
            "| | |\n" +
            "| --- | --- |\n" +
            "| A | Uses a \\| pipe |" );
    }

    [Fact]
    public void table_of_one_column_should_be_resolved_into_bullets()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Narrow ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be( "Gets or sets the narrow.\n* Only\n* One" );
    }

    [Fact]
    public void inline_code_should_be_resolved_into_a_span()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Format ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be( "Gets or sets the format, which is `yyyy-MM-dd`." );
    }

    [Fact]
    public void code_should_be_resolved_into_a_fenced_block()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Sample ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be(
            "Gets or sets the sample.\n" +
            "\n" +
            "```\n" +
            "var value = 42;\n" +
            "Use( value );\n" +
            "```" );
    }

    [Fact]
    public void code_containing_a_fence_should_be_resolved_into_a_longer_fence()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Snippet ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be(
            "Gets or sets the snippet.\n" +
            "\n" +
            "````\n" +
            "The fence is ``` here.\n" +
            "````" );
    }

    [Fact]
    public void bold_and_italic_should_be_resolved_into_emphasis()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Highlights ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be( "Gets or sets the highlights, which are **important** and *subtle*." );
    }

    [Fact]
    public void nested_emphasis_should_be_resolved_from_the_inside_out()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Emphasis ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be( "Gets or sets the emphasis, which is **very *strongly* worded**." );
    }

    [Fact]
    public void nested_emphasis_should_be_resolved_the_same_way_in_either_order()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Intraword ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be(
            "Gets or sets the intraword.\n" +
            "\n" +
            "Very***long***word\n" +
            "\n" +
            "Very***long***word" );
    }

    [Fact]
    public void tag_beside_a_paragraph_should_not_start_one()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Sibling ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be(
            "Gets or sets the sibling, which is underlined in place.\n" +
            "\n" +
            "A note." );
    }

    [Fact]
    public void space_between_adjacent_inline_tags_should_be_retained()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Adjacent ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be(
            "Gets or sets the adjacent.\n" +
            "\n" +
            "**Remark *of*** GetToDo\n" +
            "\n" +
            "**Remark**   *of*   GetToDo" );
    }

    [Fact]
    public void anchor_should_be_resolved_into_a_link()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Reference ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be(
            "Gets or sets the reference, which is described by [the specification](https://example.com)." );
    }

    [Fact]
    public void anchor_without_text_should_be_resolved_into_its_address()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Site ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be( "Gets or sets the site, which is https://example.com." );
    }

    [Fact]
    public async Task remarks_should_take_precedence_over_description()
    {
        // arrange
        var paths = await GeneratePathsAsync();

        // act
        var description = paths["/test/detailed"]["get"]["description"];

        // assert
        description.GetValue<string>().Should().Be( "The long-form explanation." );
    }

    [Fact]
    public async Task description_should_be_used_without_remarks()
    {
        // arrange
        var paths = await GeneratePathsAsync();

        // act
        var description = paths["/test/{id}"]["get"]["description"];

        // assert
        description.GetValue<string>().Should().Be( "A test API." );
    }

    [Fact]
    public void paragraphs_should_be_separated_by_a_blank_line()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var property = typeof( Documented ).GetProperty( nameof( Documented.Notes ) );

        // act
        var summary = comments.GetSummary( property );

        // assert
        summary.Should().Be( "Gets or sets the notes.\n\nThe first note.\n\nThe second note, which mentions `Status`." );
    }

    [Fact]
    public void paramref_should_be_resolved_into_the_parameter_name_as_code()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( MinimalApi ).GetMethod( nameof( MinimalApi.Echo ) );

        // act
        var returns = comments.GetReturns( method );

        // assert
        returns.Should().Be( "The value of `id`." );
    }

    [Fact]
    public async Task paramref_should_be_resolved_in_the_document()
    {
        // arrange
        var paths = await GeneratePathsAsync();

        // act
        var description = paths["/test/echo/{id}"]["get"]["responses"]["200"]["description"];

        // assert
        description.GetValue<string>().Should().Be( "The value of `id`." );
    }

    [Fact]
    public void text_around_a_code_block_should_not_be_indented()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( MinimalApi ).GetMethod( nameof( MinimalApi.Mixed ) );

        // act
        var remarks = comments.GetRemarks( method );

        // assert
        remarks.Should().Be(
            "Text before code\n" +
            "\n\n" +
            "```\n" +
            "var index = 5;\n" +
            "index++;\n" +
            "```\n" +
            "\n\n" +
            "Text after code" );
    }

    [Fact]
    public void text_around_a_list_should_not_be_indented()
    {
        // arrange
        var comments = XmlComments.FromFile( FilePath.XmlCommentFile );
        var method = typeof( MinimalApi ).GetMethod( nameof( MinimalApi.Outlined ) );

        // act
        var remarks = comments.GetRemarks( method );

        // assert
        remarks.Should().Be(
            "Text before list\n" +
            "\n" +
            "* First\n" +
            "* Second\n" +
            "\n" +
            "Text after list" );
    }

    [Fact]
    public async Task text_around_a_code_block_should_not_be_indented_in_the_document()
    {
        // arrange
        var paths = await GeneratePathsAsync();

        // act
        var description = paths["/test/mixed"]["get"]["description"].GetValue<string>();

        // assert
        description.Should().NotContain( "\n    " ).And.StartWith( "Text before code" );
    }

    [Fact]
    public async Task summary_and_value_should_describe_a_property()
    {
        // arrange
        var document = await GenerateDocumentAsync();

        // act
        var description = document["components"]["schemas"]["Documented"]["properties"]["status"]["description"];

        // assert
        description.GetValue<string>().Should().Be( "Gets or sets the status.\nDefaults to `Pending`." );
    }

    [Fact]
    public async Task value_should_describe_a_property_without_a_summary()
    {
        // arrange
        var document = await GenerateDocumentAsync();

        // act
        var description = document["components"]["schemas"]["Documented"]["properties"]["kind"]["description"];

        // assert
        description.GetValue<string>().Should().Be( "The unique identifier." );
    }

    private static async Task<JsonNode> GeneratePathsAsync() => ( await GenerateDocumentAsync() )["paths"];

    private static async Task<JsonNode> GenerateDocumentAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.WebHost.UseTestServer();
        builder.Services.AddApiVersioning()
                        .AddApiExplorer( options => options.GroupNameFormat = "'v'VVV" )
                        .AddOpenApi();

        builder.Services.AddMvcCore().ConfigureApplicationPartManager( m => m.ApplicationParts.Clear() );

        await using var app = builder.Build();
        var api = app.NewVersionedApi( "Test" ).MapGroup( "/test" ).HasApiVersion( 1.0 );

        api.MapGet( "one", MinimalApi.One );
        api.MapGet( "many", MinimalApi.Many ).Produces<int>().Produces( 400 );
        api.MapGet( "{id:int}", MinimalApi.Get ).Produces<int>().Produces( 400 );
        api.MapGet( "detailed", MinimalApi.Detailed );
        api.MapGet( "documented", () => new Documented() );
        api.MapGet( "echo/{id:int}", MinimalApi.Echo );
        api.MapGet( "mixed", MinimalApi.Mixed );
        app.MapOpenApi().WithDocumentPerVersion();

        var cancellationToken = TestContext.Current.CancellationToken;

        await app.StartAsync( cancellationToken );

        using var client = app.GetTestClient();
        var document = await client.GetFromJsonAsync<JsonNode>( "/openapi/v1.json", cancellationToken );

        await app.StopAsync( cancellationToken );

        return document;
    }
}