// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ApiController;

using Asp.Versioning;
using Asp.Versioning.Http.Basic;
using static System.Net.HttpStatusCode;

[Collection( nameof( BasicTestCollection ) )]
public class when_two_route_templates_overlap : AcceptanceTest
{
    [Fact]
    public async Task then_the_higher_precedence_route_should_be_selected_during_the_first_request()
    {
        // arrange
        var response = await GetAsync( "api/v1/values/42/children" );
        var result1 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        // act
        response = await GetAsync( "api/v1/values/42/abc" );
        var result2 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        // assert
        result1.Should().Be( "{\"id\":42}" );
        result2.Should().Be( "{\"id\":42,\"childId\":\"abc\"}" );
    }

    [Fact]
    public async Task then_the_higher_precedence_route_should_be_selected_during_the_second_request()
    {
        // arrange
        var response = await GetAsync( "api/v1/values/42/abc" );
        var result1 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        // act
        response = await GetAsync( "api/v1/values/42/children" );
        var result2 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        // assert
        result1.Should().Be( "{\"id\":42,\"childId\":\"abc\"}" );
        result2.Should().Be( "{\"id\":42}" );
    }

    [Fact]
    public async Task then_the_higher_precedence_route_should_result_in_500_during_the_second_request()
    {
        // arrange
        var response = await GetAsync( "api/v1/values/42/abc" );
        var result1 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        // act
        response = await GetAsync( "api/v1/values/42/ambiguous" );

        // assert
        result1.Should().Be( "{\"id\":42,\"childId\":\"abc\"}" );
        response.StatusCode.Should().Be( InternalServerError );
    }

    public when_two_route_templates_overlap( BasicFixture fixture ) : base( fixture ) { }
}