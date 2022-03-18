// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController;

using Asp.Versioning;
using Asp.Versioning.OData.Basic;
using static System.Net.HttpStatusCode;

public class when_using_a_url_segment : BasicAcceptanceTest
{
    [Theory]
    [InlineData( "v1/orders" )]
    [InlineData( "v1/orders/42" )]
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
        var response = await GetAsync( "v2/orders" );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( NotFound );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    public when_using_a_url_segment( BasicFixture fixture ) : base( fixture ) { }
}