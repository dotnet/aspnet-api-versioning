// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace given_a_version_neutral_ApiController;

using Asp.Versioning;
using Asp.Versioning.Http.Basic;
using static System.Net.HttpStatusCode;

[Collection( nameof( BasicTestCollection ) )]
public class when_no_version_is_specified : AcceptanceTest
{
    [Fact]
    public async Task then_get_should_return_204()
    {
        // arrange


        // act
        var response = await GetAsync( "api/ping" );

        // assert
        response.StatusCode.Should().Be( NoContent );
    }

    [Fact]
    public async Task then_post_should_return_405()
    {
        // arrange
        var entity = new { };

        // act
        var response = await PostAsync( "api/ping", entity );
        var problem = await response.Content.ReadAsProblemDetailsAsync();
        var traceId = problem.Extensions["traceId"];

        // assert
        response.Content.Headers.Allow.Should().BeEquivalentTo( "GET" );
        problem.Should().BeEquivalentTo(
            new ProblemDetails()
            {
                Status = 405,
                Title = "Unsupported API version",
                Type = ProblemDetailsDefaults.Unsupported.Type,
                Detail = "The requested resource with API version '(null)' does not support HTTP method 'POST'.",
                Extensions =
                {
                    ["code"] = ProblemDetailsDefaults.Unsupported.Code,
                    ["error"] = "No route providing a controller name with API version '(null)' was found to match HTTP method 'POST' and request URI 'http://localhost/api/ping'.",
                    ["traceId"] = traceId,
                },
            } );
    }

    public when_no_version_is_specified( BasicFixture fixture ) : base( fixture ) { }
}