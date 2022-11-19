// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_Controller;

using Asp.Versioning;
using Asp.Versioning.Mvc.UsingAttributes;
using Asp.Versioning.Mvc.UsingAttributes.Controllers;
using System.Threading.Tasks;
using static System.Net.HttpStatusCode;

[Collection( nameof( BasicTestCollection ) )]
public class when_using_a_query_string_and_split_into_two_types : AcceptanceTest
{
    [Theory]
    [InlineData( nameof( ValuesController ), "1.0" )]
    [InlineData( nameof( Values2Controller ), "2.0" )]
    public async Task then_get_should_return_200( string controller, string apiVersion )
    {
        // arrange
        var example = new { controller = "", version = "" };

        // act
        var response = await GetAsync( $"api/values?api-version={apiVersion}" );
        var content = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
        content.Should().BeEquivalentTo( new { controller, version = apiVersion } );
    }

    [Fact]
    public async Task then_get_with_string_id_should_return_200()
    {
        // arrange
        var example = new { controller = "", id = "", version = "" };

        // act
        var response = await GetAsync( $"api/values/42?api-version=1.0" );
        var content = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
        content.Should().BeEquivalentTo( new { controller = nameof( ValuesController ), id = "42", version = "1.0" } );
    }

    [Fact]
    public async Task then_get_with_integer_id_should_return_200()
    {
        // arrange
        var example = new { controller = "", id = 0, version = "" };

        // act
        var response = await GetAsync( $"api/values/42?api-version=2.0" );
        var content = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
        content.Should().BeEquivalentTo( new { controller = nameof( Values2Controller ), id = 42, version = "2.0" } );
    }

    [Fact]
    public async Task then_get_returns_404_with_invalid_id()
    {
        // arrange
        var requestUrl = "api/values/abc?api-version=2.0";

        // act
        var response = await GetAsync( requestUrl );

        // assert
        response.StatusCode.Should().Be( NotFound );
    }

    [Theory]
    [InlineData( "1.0" )]
    [InlineData( "2.0" )]
    public async Task then_delete_should_return_405( string apiVersion )
    {
        // arrange
        var requestUrl = $"api/values/42?api-version={apiVersion}";

        // act
        var response = await DeleteAsync( requestUrl );

        // assert
        response.StatusCode.Should().Be( MethodNotAllowed );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unsupported_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/values?api-version=3.0" );

        // assert
        response.StatusCode.Should().Be( BadRequest );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unspecified_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/values" );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( BadRequest );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unspecified.Type );
    }

    [Theory]
    [InlineData( nameof( ValuesController ), "1.0" )]
    [InlineData( nameof( Values2Controller ), "2.0" )]
    public async Task then_action_segment_should_not_be_ambiguous_with_route_parameter( string controller, string apiVersion )
    {
        // arrange
        var example = new { controller = "", query = "", version = "" };

        // act
        var response = await GetAsync( $"api/values/search?query=Foo&api-version={apiVersion}" );
        var content = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
        content.Should().BeEquivalentTo( new { controller, query = "Foo", version = apiVersion } );
    }

    public when_using_a_query_string_and_split_into_two_types( BasicFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}