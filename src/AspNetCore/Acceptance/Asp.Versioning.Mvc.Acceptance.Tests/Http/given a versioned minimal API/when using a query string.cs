// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_minimal_API;

using Asp.Versioning;
using Asp.Versioning.Http;
using static System.Net.HttpStatusCode;

[Collection( nameof( MinimalApiTestCollection ) )]
public class when_using_a_query_string : AcceptanceTest
{
    [Theory]
    [InlineData( 1 )]
    [InlineData( 2 )]
    public async Task then_get_should_return_200( int version )
    {
        // arrange


        // act
        var response = await GetAsync( $"api/values?api-version={version}.0" );
        var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync( CancellationToken );

        // assert
        result.Should().Be( "Value " + version );
    }

    [Fact]
    public async Task then_get_should_report_api_versions()
    {
        // arrange


        // act
        var response = await GetAsync( "api/values?api-version=1.0" );

        // assert
        response.StatusCode.Should().Be( OK );
        response.Headers.GetValues( "api-supported-versions" ).Should().Equal( "1.0, 2.0" );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unsupported_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/values?api-version=3.0" );
        var problem = await response.Content.ReadAsProblemDetailsAsync( CancellationToken );

        // assert
        response.StatusCode.Should().Be( BadRequest );
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unspecified_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/values" );
        var problem = await response.Content.ReadAsProblemDetailsAsync( CancellationToken );

        // assert
        response.StatusCode.Should().Be( BadRequest );
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unspecified.Type );
    }

    [Fact]
    public async Task then_get_should_return_400_for_a_malformed_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/values?api-version=abc" );
        var problem = await response.Content.ReadAsProblemDetailsAsync( CancellationToken );

        // assert
        response.StatusCode.Should().Be( BadRequest );
        response.Headers.GetValues( "api-supported-versions" ).Should().Equal( "1.0, 2.0" );
        problem.Type.Should().Be( ProblemDetailsDefaults.Invalid.Type );
    }

    public when_using_a_query_string( MinimalApiFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}