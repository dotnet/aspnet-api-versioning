namespace given_a_version_neutral_ApiController
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.Http.Basic;
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
            content.Error.ShouldBeEquivalentTo(
                new
                {
                    Code = "UnsupportedApiVersion",
                    InnerError = new
                    {
                        Code = default( string ),
                        Message = "No route providing a controller name with API version '(null)' was found to match request URI 'http://localhost/api/ping'."
                    },
                    Message = "The HTTP resource that matches the request URI 'http://localhost/api/ping' does not support the API version '(null)'."
                } );
        }
    }
}