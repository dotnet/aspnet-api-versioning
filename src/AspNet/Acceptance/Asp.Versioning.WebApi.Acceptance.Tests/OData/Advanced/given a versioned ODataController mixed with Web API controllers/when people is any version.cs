// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_ODataController_mixed_with_Web_API_controllers;

using Asp.Versioning;
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
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( BadRequest );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    public when_people_is_any_version( AdvancedFixture fixture ) : base( fixture ) { }
}