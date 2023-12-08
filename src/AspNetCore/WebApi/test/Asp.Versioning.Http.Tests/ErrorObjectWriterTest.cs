// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;

public class ErrorObjectWriterTest
{
    [Theory]
    [InlineData( "https://docs.api-versioning.org/problems#unsupported" )]
    [InlineData( "https://docs.api-versioning.org/problems#unspecified" )]
    [InlineData( "https://docs.api-versioning.org/problems#invalid" )]
    [InlineData( "https://docs.api-versioning.org/problems#ambiguous" )]
    public void can_write_should_be_true_for_api_versioning_problem_types( string type )
    {
        // arrange
        var writer = new ErrorObjectWriter( Options.Create( new JsonOptions() ) );
        var context = new ProblemDetailsContext()
        {
            HttpContext = new DefaultHttpContext(),
            ProblemDetails =
            {
                Type = type,
            },
        };

        // act
        var result = writer.CanWrite( context );

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void can_write_should_be_false_for_other_problem_types()
    {
        // arrange
        const string BadRequest = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
        var writer = new ErrorObjectWriter( Options.Create( new JsonOptions() ) );
        var context = new ProblemDetailsContext()
        {
            HttpContext = new DefaultHttpContext(),
            ProblemDetails =
            {
                Type = BadRequest,
            },
        };

        // act
        var result = writer.CanWrite( context );

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task write_async_should_output_expected_json()
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

        var writer = new ErrorObjectWriter( Options.Create( new JsonOptions() ) );
        using var stream = new MemoryStream();
        var response = new Mock<HttpResponse>() { CallBase = true };
        var httpContext = new Mock<HttpContext>() { CallBase = true };

        response.SetupGet( r => r.Body ).Returns( stream );
        response.SetupProperty( r => r.ContentType );
        response.SetupGet( r => r.HttpContext ).Returns( () => httpContext.Object );
        httpContext.SetupGet( c => c.Response ).Returns( response.Object );

        var context = new ProblemDetailsContext()
        {
            HttpContext = httpContext.Object,
            ProblemDetails =
            {
                Type = ProblemDetailsDefaults.Unsupported.Type,
                Title = ProblemDetailsDefaults.Unsupported.Title,
                Status = 400,
                Detail = "The HTTP resource that matches the request URI 'https://tempuri.org' does not support the API version '42.0'.",
                Extensions =
                {
                    ["code"] = ProblemDetailsDefaults.Unsupported.Code,
                },
            },
        };

        // act
        await writer.WriteAsync( context );

        await stream.FlushAsync();
        stream.Position = 0;

        var error = await DeserializeByExampleAsync( stream, example );

        // assert
        response.Object.ContentType.Should().Be( "application/json; charset=utf-8" );
        error.Should().BeEquivalentTo(
            new
            {
                error = new
                {
                    code = "UnsupportedApiVersion",
                    message = "Unsupported API version",
                    innerError = new
                    {
                        message = "The HTTP resource that matches the request URI 'https://tempuri.org' does not support the API version '42.0'.",
                    },
                },
            } );
    }

#pragma warning disable IDE0060 // Remove unused parameter
    private static ValueTask<T> DeserializeByExampleAsync<T>( Stream stream, T example ) =>
        JsonSerializer.DeserializeAsync<T>( stream );
#pragma warning restore IDE0060 // Remove unused parameter
}