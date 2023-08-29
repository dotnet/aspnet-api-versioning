// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_versioned_minimal_API;

using Asp.Versioning;
using Asp.Versioning.Http;

public class when_error_objects_are_enabled : AcceptanceTest, IClassFixture<InteropFixture>
{
    [Fact]
    public async Task then_the_response_should_not_be_problem_details()
    {
        // arrange
        var example = new
        {
            error = new
            {
                code = default( string ),
                message = default( string ),
                target = default( string ),
                innerError = new
                {
                    message = default( string ),
                },
            },
        };

        // act
        var response = await GetAsync( "api/values?api-version=3.0" );
        var error = await response.Content.ReadAsExampleAsync( example );

        // assert
        response.Content.Headers.ContentType.MediaType.Should().Be( "application/json" );
        error.Should().BeEquivalentTo(
            new
            {
                error = new
                {
                    code = "UnsupportedApiVersion",
                    message = "Unsupported API version",
                    innerError = new
                    {
                        message = "The HTTP resource that matches the request URI " +
                                  "'http://localhost/api/values' does not support " +
                                  "the API version '3.0'.",
                    },
                },
            } );
    }

    public when_error_objects_are_enabled( InteropFixture fixture ) : base( fixture ) { }
}