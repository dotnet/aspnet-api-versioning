// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController_mixed_with_Web_API_controllers;

using Asp.Versioning;
using Asp.Versioning.OData.Advanced;
using static System.Net.HttpStatusCode;

public class when_people_is_v3 : AdvancedAcceptanceTest
{
    [Fact]
    public async Task then_get_should_return_200()
    {
        // arrange
        var example = new { value = new[] { new { id = 0, firstName = "", lastName = "", email = "", phone = "" } } };

        // act
        var response = await GetAsync( "api/people?api-version=3.0" );
        var people = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        people.value.Should().BeEquivalentTo(
            new[] { new { id = 1, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com", phone = "555-555-5555" } },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public async Task then_get_with_key_should_return_200()
    {
        // arrange
        var example = new { id = 0, firstName = "", lastName = "", email = "", phone = "" };

        // act
        var response = await GetAsync( "api/people/42?api-version=3.0" );
        var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        order.Should().BeEquivalentTo(
            new { id = 42, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com", phone = "555-555-5555" },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public async Task then_patch_should_return_405_if_supported_in_any_version()
    {
        // arrange
        var person = new { lastName = "Me" };

        // act
        var response = await PatchAsync( $"api/people/42?api-version=3.0", person );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( MethodNotAllowed );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    public when_people_is_v3( AdvancedFixture fixture ) : base( fixture ) { }
}