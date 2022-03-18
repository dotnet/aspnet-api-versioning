// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versionX2Dneutral_minimal_API;

using Asp.Versioning;
using Asp.Versioning.Http;

[Collection( nameof( MinimalApiTestCollection ) )]
public class when_no_version_is_specified : AcceptanceTest
{
    [Fact]
    public async Task then_get_should_return_204()
    {
        // arrange


        // act
        var response = await GetAsync( "api/ping" );

        // assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task then_delete_should_explicitly_map_to_endpoint()
    {
        // arrange


        // act
        var response = await DeleteAsync( "api/order/42" );

        // assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    public when_no_version_is_specified( MinimalApiFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}