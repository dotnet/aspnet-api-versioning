// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http;

using Asp.Versioning;
using System.Net.Http;
using System.Net.Http.Json;
using static System.Net.Http.Headers.MediaTypeWithQualityHeaderValue;
using static System.Net.Http.HttpMethod;
using static System.Net.HttpStatusCode;

public class when_using_a_media_type : AcceptanceTest, IClassFixture<MediaTypeFixture>
{
    [Fact]
    public async Task problem_details_should_be_returned_for_accept_header_with_unsupported_api_version()
    {
        // arrange
        using var request = new HttpRequestMessage( Post, "api/values" )
        {
            Headers = { Accept = { Parse( "application/json;v=3.0" ) } },
            Content = JsonContent.Create( new { test = true }, Parse( "application/json;v=3.0" ) ),
        };

        // act
        var response = await Client.SendAsync( request );
        var problem = await response.Content.ReadAsProblemDetailsAsync();

        // assert
        response.StatusCode.Should().Be( UnsupportedMediaType );
        problem.Type.Should().Be( ProblemDetailsDefaults.Unsupported.Type );
    }

    public when_using_a_media_type( MediaTypeFixture fixture, ITestOutputHelper console )
        : base( fixture ) => console.WriteLine( fixture.DirectedGraphVisualizationUrl );
}