﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_Controller_using_conventions;

using Asp.Versioning;
using Asp.Versioning.Mvc.UsingConventions;
using static System.Net.HttpStatusCode;

[Collection( nameof( ConventionsTestCollection ) )]
public class when_using_an_action : AcceptanceTest
{
    [Theory]
    [InlineData( "api/orders/42?api-version=0.9" )]
    [InlineData( "api/orders/42?api-version=1.0" )]
    [InlineData( "api/orders/42?api-version=2.0" )]
    [InlineData( "api/orders?api-version=1.0" )]
    [InlineData( "api/orders?api-version=2.0" )]
    public async Task then_get_should_return_200( string requestUrl )
    {
        // arrange

        // act
        var response = await GetAsync( requestUrl );

        // assert
        response.StatusCode.Should().Be( OK );
    }

    [Theory]
    [InlineData( "api/orders?api-version=1.0" )]
    [InlineData( "api/orders?api-version=2.0" )]
    public async Task then_post_should_return_201( string requestUrl )
    {
        // arrange
        var content = new { customer = "Bill Mei" };

        // act
        var response = await PostAsync( requestUrl, content );

        // assert
        response.StatusCode.Should().Be( Created );
        response.Headers.Location.Should().Be( new Uri( "http://localhost/api/Orders/42" ) );
    }

    [Fact]
    public async Task then_put_should_return_204()
    {
        // arrange
        var requestUrl = "api/orders/42?api-version=2.0";
        var content = new { customer = "Bill Mei" };

        // act
        var response = await PutAsync( requestUrl, content );

        // assert
        response.StatusCode.Should().Be( NoContent );
    }

    [Theory]
    [InlineData( "api/orders/42" )]
    [InlineData( "api/orders/42?api-version=0.9" )]
    [InlineData( "api/orders/42?api-version=1.0" )]
    [InlineData( "api/orders/42?api-version=2.0" )]
    public async Task then_delete_should_return_204( string requestUrl )
    {
        // arrange

        // act
        var response = await DeleteAsync( requestUrl );

        // assert
        response.StatusCode.Should().Be( NoContent );
    }

    public when_using_an_action( ConventionsFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}