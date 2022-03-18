// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_minimal_API;

using Asp.Versioning;
using Asp.Versioning.Http;
using static System.Net.Http.HttpMethod;

[Collection( nameof( MinimalApiTestCollection ) )]
public class when_using_a_url_segment : AcceptanceTest
{
    [Theory]
    [InlineData( "v1", "Hello world!" )]
    [InlineData( "v2", "Hello world! (v2)" )]
    public async Task then_get_should_map_to_api_version( string apiVersion, string expected )
    {
        // arrange


        // act
        var response = await GetAsync( $"api/{apiVersion}/hello" );
        var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "v1", "Hi" )]
    [InlineData( "v2", "Hi (v2)" )]
    public async Task hello_world_get_with_key_should_map_to_api_version( string apiVersion, string expected )
    {
        // arrange


        // act
        var response = await GetAsync( $"api/{apiVersion}/hello/Hi" );
        var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( "v1" )]
    [InlineData( "v2" )]
    public async Task then_post_should_map_to_api_version( string apiVersion )
    {
        // arrange
        using var request = new HttpRequestMessage( Post, $"api/{apiVersion}/hello" );

        // act
        var response = await Client.SendAsync( request );

        // assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task then_post_should_report_api_versions()
    {
        // arrange
        using var request = new HttpRequestMessage( Post, "api/v1/hello" );

        // act
        var response = await Client.SendAsync( request );

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Should().Equal( "1.0, 2.0" );
    }

    public when_using_a_url_segment( MinimalApiFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}