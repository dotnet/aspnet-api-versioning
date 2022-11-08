// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_Controller;

using Asp.Versioning;
using Asp.Versioning.Mvc.UsingAttributes;
using System.Net;
using static System.Net.HttpStatusCode;

public class when_two_route_templates_overlap : AcceptanceTest, IClassFixture<OverlappingRouteTemplateFixture>
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
    public async Task then_the_higher_precedence_route_should_result_in_ambiguous_action_exception_during_the_second_request()
    {
        // arrange
        var response = await GetAsync( "api/v1/values/42/abc" );
        var result1 = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

        // act
        Func<Task> act = async () => await GetAsync( "api/v1/values/42/ambiguous" );

        // assert
        result1.Should().Be( "{\"id\":42,\"childId\":\"abc\"}" );
        ( await act.Should().ThrowAsync<Exception>() ).And.GetType().Name.Should().Be( "AmbiguousMatchException" );
    }

    [Theory]
    [InlineData( "api/v1/values/echo" )]
    [InlineData( "api/v2/values/echo" )]
    public async Task then_route_with_same_score_and_version_should_return_200( string requestUri )
    {
        // arrange


        // act
        var response = await GetAsync( requestUri );

        // assert
        response.StatusCode.Should().Be( OK );
    }

    [Theory]
    [InlineData( "api/v1/values/echo/42", OK )]
    [InlineData( "api/v2/values/echo/42", NotFound )]
    public async Task then_route_with_same_score_and_different_versions_should_return_expected_status( string requestUri, HttpStatusCode statusCode )
    {
        // arrange


        // act
        var response = await GetAsync( requestUri );

        // assert
        response.StatusCode.Should().Be( statusCode );
    }

    public when_two_route_templates_overlap( OverlappingRouteTemplateFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}