// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_minimal_API;

using Asp.Versioning;
using Asp.Versioning.Http;
using System.Net;
using static System.Net.HttpStatusCode;

[Collection( nameof( MinimalApiTestCollection ) )]
public class when_using_an_endpoint : AcceptanceTest
{
    [Theory]
    [InlineData( "api/order?api-version=0.9", BadRequest )]
    [InlineData( "api/order?api-version=1.0", OK )]
    [InlineData( "api/order?api-version=2.0", OK )]
    [InlineData( "api/order/42?api-version=0.9", OK )]
    [InlineData( "api/order/42?api-version=1.0", OK )]
    [InlineData( "api/order/42?api-version=2.0", OK )]
    public async Task then_get_should_return_expected_status_code( string requestUri, HttpStatusCode statusCode )
    {
        // arrange


        // act
        var response = await GetAsync( requestUri );

        // assert
        response.StatusCode.Should().Be( statusCode );
    }

    public when_using_an_endpoint( MinimalApiFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}