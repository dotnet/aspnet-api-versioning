// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versionX2Dneutral_minimal_API;

using Asp.Versioning;
using Asp.Versioning.Http;

[Collection( nameof( MinimalApiTestCollection ) )]
public class when_any_version_is_specified : AcceptanceTest
{
    [Theory]
    [InlineData( "0.9" )]
    [InlineData( "1.0" )]
    [InlineData( "2.0" )]
    public async Task then_delete_should_succeed( string apiVersion )
    {
        // arrange


        // act
        var response = await DeleteAsync( "api/order/42?api-version=" + apiVersion );

        // assert
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    public when_any_version_is_specified( MinimalApiFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}