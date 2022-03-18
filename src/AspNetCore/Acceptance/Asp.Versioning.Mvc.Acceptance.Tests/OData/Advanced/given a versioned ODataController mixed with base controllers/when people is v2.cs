// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController_mixed_with_base_controllers;

using Asp.Versioning;
using Asp.Versioning.OData.Advanced;
using static System.Net.HttpStatusCode;

public class when_people_is_v2 : AdvancedAcceptanceTest
{
    [Fact]
    public async Task then_get_should_return_200()
    {
        // arrange
        var example = new { value = new[] { new { id = 0, firstName = "", lastName = "", email = "" } } };

        // act
        var response = await GetAsync( "api/people?api-version=2.0" );
        var people = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        people.value.Should().BeEquivalentTo(
            new[] { new { id = 1, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com" } },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public async Task then_get_with_key_should_return_200()
    {
        // arrange
        var example = new { id = 0, firstName = "", lastName = "", email = "" };

        // act
        var response = await GetAsync( "api/people/42?api-version=2.0" );
        var order = await response.EnsureSuccessStatusCode().Content.ReadAsExampleAsync( example );

        // assert
        order.Should().BeEquivalentTo(
            new { id = 42, firstName = "Bill", lastName = "Mei", email = "bill.mei@somewhere.com" },
            options => options.ExcludingMissingMembers() );
    }

    [Fact]
    public async Task then_patch_should_return_204()
    {
        // arrange
        var person = new { email = "bmei@somewhere.com" };

        // act
        var response = await PatchAsync( "api/people/42?api-version=2.0", person );

        // assert
        response.StatusCode.Should().Be( NoContent );
    }

    [Fact]
    public async Task then_patch_should_return_400_while_updating_member_that_does_not_exist_in_version()
    {
        // arrange
        var person = new { phone = "bmei@somewhere.com" };

        // act
        var response = await PatchAsync( "api/people/42?api-version=2.0", person );

        // assert
        response.StatusCode.Should().Be( BadRequest );
    }

    public when_people_is_v2( AdvancedFixture fixture ) : base( fixture ) { }
}