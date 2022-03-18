// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versionX2Dneutral_Controller;

using Asp.Versioning;
using Asp.Versioning.Mvc.UsingAttributes;
using Microsoft.AspNetCore.Mvc;
using static System.Net.HttpStatusCode;

[Collection( nameof( BasicTestCollection ) )]
public class when_no_version_is_specified : AcceptanceTest
{
    [Fact]
    public async Task then_get_should_return_204()
    {
        // arrange


        // act
        var response = await GetAsync( "api/ping" );

        // assert
        response.StatusCode.Should().Be( NoContent );
    }

    [Fact]
    public async Task then_post_should_return_405()
    {
        // arrange
        var entity = new { };

        // act
        var response = await PostAsync( "api/ping", entity );

        // assert
        response.StatusCode.Should().Be( MethodNotAllowed );
        response.Content.Headers.Allow.Should().BeEquivalentTo( "GET" );
    }

    public when_no_version_is_specified( BasicFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}