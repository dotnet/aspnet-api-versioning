// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController_mixed_with_Web_API_controllers;

using Asp.Versioning;
using Asp.Versioning.OData.Advanced;
using static System.Net.HttpStatusCode;

public class when_people_is_v1 : AdvancedAcceptanceTest
{
    [Theory]
    [InlineData( "api/people" )]
    [InlineData( "api/people?api-version=1.0" )]
    public async Task then_get_should_return_200( string requestUrl )
    {
        // arrange
        var example = new { value = new[] { new { id = 0, firstName = "", lastName = "" } } };

        // act
        var response = await GetAsync( requestUrl );
        var people = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        people.value.Should().BeEquivalentTo(
            new[] { new { id = 1, firstName = "Bill", lastName = "Mei" } },
            options => options.ExcludingMissingMembers() );
    }

    [Theory]
    [InlineData( "api/people/42" )]
    [InlineData( "api/people/42?api-version=1.0" )]
    public async Task then_get_with_key_should_return_200( string requestUrl )
    {
        // arrange
        var example = new { id = 0, firstName = "", lastName = "" };

        // act
        var response = await GetAsync( requestUrl );
        var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        order.Should().BeEquivalentTo(
            new { id = 42, firstName = "Bill", lastName = "Mei" },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public async Task then_patch_should_return_405_if_supported_in_any_version()
    {
        // arrange
        var person = new { lastName = "Me" };

        // act
        var response = await PatchAsync( $"api/people/42?api-version=1.0", person );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( MethodNotAllowed );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    public when_people_is_v1( AdvancedFixture fixture ) : base( fixture ) { }
}