// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_Controller;

using Asp.Versioning;
using Asp.Versioning.Mvc.UsingMediaType;
using Asp.Versioning.Mvc.UsingMediaType.Controllers;
using System.Net.Http;
using System.Net.Http.Json;
using static System.Net.Http.Headers.MediaTypeWithQualityHeaderValue;
using static System.Net.Http.HttpMethod;
using static System.Net.HttpStatusCode;
using static System.Text.Encoding;

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
            Headers =
            {
                Accept =
                {
                    Parse( "application/json;v=3.0" ),
                    Parse( ProblemDetailsDefaults.MediaType.Json ),
                },
            },
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
        using var request = new HttpRequestMessage( Post, "api/values" )
        {
            Content = JsonContent.Create( new { test = true }, Parse( "application/json;v=3.0" ) ),
        };

        // act
        var response = await Client.SendAsync( request );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( UnsupportedMediaType );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    [Fact]
    public async Task then_patch_should_return_415_for_a_supported_version_and_unsupported_media_type()
    {
        // arrange
        using var request = new HttpRequestMessage( Patch, "api/values/42" )
        {
            Content = JsonContent.Create( new { test = true }, Parse( "application/json;v=2.0" ) ),
        };

        // act
        var response = await Client.SendAsync( request );

        // assert
        response.StatusCode.Should().Be( UnsupportedMediaType );
    }

    [Theory]
    [InlineData( "api/values", nameof( Values2Controller ), "2.0" )]
    [InlineData( "api/helloworld", nameof( HelloWorldController ), "1.0" )]
    public async Task then_get_should_return_current_version_for_an_unspecified_version( string requestUrl, string controller, string apiVersion )
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
        using var content = new StringContent( "{\"text\":\"Test\"}", UTF8 );

        content.Headers.ContentType = Parse( "application/json;v=1.0" );

        // act
        var response = await PostAsync( "api/helloworld", content );

        // assert
        response.StatusCode.Should().Be( Created );
        response.Headers.Location.Should().Be( new Uri( "http://localhost/api/HelloWorld/42" ) );
    }

    public when_using_media_type_negotiation( MediaTypeFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}