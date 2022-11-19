// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_Controller_using_conventions;

using Asp.Versioning;
using Asp.Versioning.Mvc.UsingConventions;
using Asp.Versioning.Mvc.UsingConventions.Controllers;
using static System.Net.HttpStatusCode;

[Collection( nameof( ConventionsTestCollection ) )]
public class when_using_a_query_string_and_split_into_two_types : AcceptanceTest
{
    [Theory]
    [InlineData( nameof( ValuesController ), "1.0" )]
    [InlineData( nameof( Values2Controller ), "2.0" )]
    [InlineData( nameof( Values2Controller ), "3.0" )]
    public async Task then_get_should_return_200( string controller, string apiVersion )
    {
        // arrange
        var example = new { controller = "", version = "" };

        // act
        var response = await GetAsync( $"api/values?api-version={apiVersion}" );
        var content = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
        content.Should().BeEquivalentTo( new { controller, version = apiVersion } );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unsupported_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/values?api-version=4.0" );

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

    public when_using_a_query_string_and_split_into_two_types( ConventionsFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}