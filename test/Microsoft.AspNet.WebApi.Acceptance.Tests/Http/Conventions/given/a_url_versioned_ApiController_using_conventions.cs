namespace given
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.Http.Conventions;
    using Microsoft.Web.Http.Conventions.Controllers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class _a_url_versioned_ApiController_using_conventions : ConventionsAcceptanceTest
    {
        [Theory]
        [InlineData( "api/v1/helloworld", nameof( HelloWorldController ), "1" )]
        [InlineData( "api/v2/helloworld", nameof( HelloWorld2Controller ), "2" )]
        [InlineData( "api/v3/helloworld", nameof( HelloWorld2Controller ), "3" )]
        public async Task _get_should_return_200( string requestUrl, string controllerName, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "" };

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "2.0, 3.0, 4.0" );
            response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "1.0" );
            content.ShouldBeEquivalentTo( new { controller = controllerName, version = apiVersion } );
        }

        [Theory]
        [InlineData( "api/v1/helloworld/42", nameof( HelloWorldController ), "1" )]
        [InlineData( "api/v2/helloworld/42", nameof( HelloWorld2Controller ), "2" )]
        [InlineData( "api/v3/helloworld/42", nameof( HelloWorld2Controller ), "3" )]
        public async Task _get_with_id_should_return_200( string requestUrl, string controllerName, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "", id = "" };

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "2.0, 3.0, 4.0" );
            response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "1.0" );
            content.ShouldBeEquivalentTo( new { controller = controllerName, version = apiVersion, id = "42" } );
        }

        [Fact]
        public async Task _get_should_return_400_when_version_is_unsupported()
        {
            // arrange


            // act
            var response = await GetAsync( "api/v4/helloworld" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}