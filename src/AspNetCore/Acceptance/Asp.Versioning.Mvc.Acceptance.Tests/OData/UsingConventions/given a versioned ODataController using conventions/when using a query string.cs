﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController_using_conventions;

using Asp.Versioning;
using Asp.Versioning.OData.UsingConventions;
using static System.Net.HttpStatusCode;

public class when_using_a_query_string : ConventionsAcceptanceTest
{
    [Theory]
    [InlineData( "api/orders?api-version=1.0" )]
    [InlineData( "api/orders/42?api-version=1.0" )]
    public async Task then_get_should_return_200( string requestUrl )
    {
        // arrange


        // act
        var response = ( await GetAsync( requestUrl ) ).EnsureSuccessStatusCode();

        // assert
        response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0" );
    }

    [Fact]
    public async Task then_get_should_return_404_for_an_unsupported_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/orders?api-version=2.0" );

        // assert
        response.StatusCode.Should().Be( NotFound );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unspecified_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/orders" );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( BadRequest );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unspecified.Type );
    }

    public when_using_a_query_string( ConventionsFixture fixture ) : base( fixture ) { }
}