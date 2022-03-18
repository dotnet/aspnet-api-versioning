// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_minimal_API;

using Asp.Versioning;
using Asp.Versioning.Http;

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
        var result = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

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
        response.Headers.GetValues( "api-supported-versions" ).Should().Equal( "1.0, 2.0" );
    }

    public when_using_a_query_string( MinimalApiFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}