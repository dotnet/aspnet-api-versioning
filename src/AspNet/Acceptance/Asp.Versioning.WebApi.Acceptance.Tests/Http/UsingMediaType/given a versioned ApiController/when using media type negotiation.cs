// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ApiController;

using Asp.Versioning;
using Asp.Versioning.Http.UsingMediaType;
using Asp.Versioning.Http.UsingMediaType.Controllers;
using System.Net.Http;
using System.Net.Http.Formatting;
using static System.Net.Http.Headers.MediaTypeWithQualityHeaderValue;
using static System.Net.Http.HttpMethod;
using static System.Net.HttpStatusCode;

public class when_using_media_type_negotiation : AcceptanceTest, IClassFixture<MediaTypeFixture>
{
    [Theory]
    [InlineData( nameof( ValuesController ), "1.0" )]
    [InlineData( nameof( Values2Controller ), "2.0" )]
    public async Task then_get_should_return_200( string controller, string apiVersion )
    {
        // arrange
        var example = new { controller = "", version = "" };
        using var request = new HttpRequestMessage( Get, "api/values" )
        {
            Headers = { Accept = { Parse( "application/json;v=" + apiVersion ) } },
        };

        // act
        var response = await Client.SendAsync( request );
        var body = response.EnsureSuccessStatusCode().Content;
        var content = await body.ReadAsExampleAsync( example );

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
        body.Headers.ContentType.Parameters.Single( p => p.Name == "v" ).Value.Should().Be( apiVersion );
        content.Should().BeEquivalentTo( new { controller, version = apiVersion } );
    }

    [Fact]
    public async Task then_get_should_return_406_for_an_unsupported_version()
    {
        // arrange
        using var request = new HttpRequestMessage( Get, "api/values" )
        {
            Headers = { Accept = { Parse( "application/json;v=3.0" ) } },
        };

        // act
        var response = await Client.SendAsync( request );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( NotAcceptable );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    [Fact]
    public async Task then_post_should_return_415_for_an_unsupported_version()
    {
        // arrange
        var entity = new { text = "Test" };
        var mediaType = Parse( "application/json;v=3.0" );
        using var content = new ObjectContent( entity.GetType(), entity, new JsonMediaTypeFormatter(), mediaType );

        // act
        var response = await Client.PostAsync( "api/values", content );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( UnsupportedMediaType );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    [Theory]
    [InlineData( "api/values", nameof( Values2Controller ), "2.0" )]
    [InlineData( "api/helloworld", nameof( HelloWorldController ), "1.0" )]
    public async Task then_get_should_allow_an_unspecified_version( string requestUrl, string controller, string apiVersion )
    {
        // arrange
        var example = new { controller = "", version = "" };

        // act
        var response = await GetAsync( requestUrl );
        var body = response.EnsureSuccessStatusCode().Content;
        var content = await body.ReadAsExampleAsync( example );

        // assert
        body.Headers.ContentType.Parameters.Single( p => p.Name == "v" ).Value.Should().Be( apiVersion );
        content.Should().BeEquivalentTo( new { controller, version = apiVersion } );
    }

    [Fact]
    public async Task then_post_should_return_201()
    {
        // arrange
        var entity = new { text = "Test" };
        var mediaType = Parse( "application/json;v=1.0" );
        using var content = new ObjectContent( entity.GetType(), entity, new JsonMediaTypeFormatter(), mediaType );

        // act
        var response = await PostAsync( "api/helloworld", content );

        // assert
        response.StatusCode.Should().Be( Created );
        response.Headers.Location.Should().Be( new Uri( "http://localhost/api/helloworld/42" ) );
    }

    public when_using_media_type_negotiation( MediaTypeFixture fixture ) : base( fixture ) { }
}