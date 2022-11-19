// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController_mixed_with_base_controllers;

using Asp.Versioning.OData.Advanced;
using static System.Net.HttpStatusCode;

public class when_people_is_any_version : AdvancedAcceptanceTest
{
    [Fact]
    public async Task then_patch_should_return_400_for_an_unsupported_version()
    {
        // arrange
        var person = new { lastName = "Me" };

        // act
        var response = await PatchAsync( $"api/people/42?api-version=4.0", person );

        // assert
        response.StatusCode.Should().Be( BadRequest );
    }

    [Fact]
    public async Task then_delete_should_return_405()
    {
        // arrange


        // act
        var response = await DeleteAsync( $"api/people/42?api-version=1.0" );

        // assert
        response.StatusCode.Should().Be( MethodNotAllowed );
    }

    public when_people_is_any_version( AdvancedFixture fixture ) : base( fixture ) { }
}