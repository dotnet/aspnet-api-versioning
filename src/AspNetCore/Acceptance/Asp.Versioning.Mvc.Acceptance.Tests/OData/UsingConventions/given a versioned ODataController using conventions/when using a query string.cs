// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController_using_conventions;

using Asp.Versioning;
using Asp.Versioning.OData.UsingConventions;
using System.Net;
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
    public async Task then_get_should_return_400_for_an_unsupported_version()
    {
        // arrange
        // note: it's not clear why this is, but it appears to be a change
        // in the routing system from netcoreapp3.1 to net6.0+
#if NETCOREAPP3_1
        const HttpStatusCode StatusCode = NotFound;
#else
        const HttpStatusCode StatusCode = BadRequest;
#endif

        // act
        var response = await GetAsync( "api/orders?api-version=2.0" );

        // assert
        response.StatusCode.Should().Be( StatusCode );
    }

    [Fact]
    public async Task then_get_should_return_400_for_an_unspecified_version()
    {
        // arrange


        // act
        var response = await GetAsync( "api/orders" );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert

        // change from 3.1 to 6.0; DELETE is version-neutral
        // and the only candidate, so GET returns 405
#if NETCOREAPP3_1
        response.StatusCode.Should().Be( MethodNotAllowed );
#else
        response.StatusCode.Should().Be( BadRequest );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unspecified.Type );
#endif
    }

    public when_using_a_query_string( ConventionsFixture fixture ) : base( fixture ) { }
}