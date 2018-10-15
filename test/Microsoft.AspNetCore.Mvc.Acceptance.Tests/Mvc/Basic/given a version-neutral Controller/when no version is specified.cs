namespace given_a_versionX2Dneutral_Controller
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Basic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class when_no_version_is_specified : BasicAcceptanceTest
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
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( MethodNotAllowed );
            response.Content.Headers.Allow.Should().BeEquivalentTo( "GET" );
            content.Error.Should().BeEquivalentTo(
                new
                {
                    Code = "UnsupportedApiVersion",
                    InnerError = default( OneApiInnerError ),
                    Message = "The HTTP resource that matches the request URI 'http://localhost/api/ping' does not support HTTP method 'POST'."
                } );
        }

        [Fact]
        public async Task then_unsupported_api_version_error_should_only_contain_the_path()
        {
            // arrange
            var entity = new { };

            // act
            var response = await PostAsync( "api/ping?additionalQuery=true", entity );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( MethodNotAllowed );
            response.Content.Headers.Allow.Should().BeEquivalentTo( "GET" );
            content.Error.Should().BeEquivalentTo(
                new
                {
                    Code = "UnsupportedApiVersion",
                    InnerError = default( OneApiInnerError ),
                    Message = "The HTTP resource that matches the request URI 'http://localhost/api/ping' does not support HTTP method 'POST'."
                } );
        }
    }
}

