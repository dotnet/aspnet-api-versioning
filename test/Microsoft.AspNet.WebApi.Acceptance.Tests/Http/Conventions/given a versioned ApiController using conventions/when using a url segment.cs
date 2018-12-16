namespace given_a_versioned_ApiController_using_conventions
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.Http.Conventions;
    using Microsoft.Web.Http.Conventions.Controllers;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Collection( nameof( ConventionsCollection ) )]
    public class when_using_a_url_segment : AcceptanceTest
    {
        [Theory]
        [InlineData( "api/v1/helloworld", nameof( HelloWorldController ), "1" )]
        [InlineData( "api/v2/helloworld", nameof( HelloWorld2Controller ), "2" )]
        [InlineData( "api/v3/helloworld", nameof( HelloWorld2Controller ), "3" )]
        public async Task then_get_should_return_200( string requestUrl, string controllerName, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "" };

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "2.0, 3.0, 4.0" );
            response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "1.0" );
            content.Should().BeEquivalentTo( new { controller = controllerName, version = apiVersion } );
        }

        [Theory]
        [InlineData( "api/v1/helloworld/42", nameof( HelloWorldController ), "1" )]
        [InlineData( "api/v2/helloworld/42", nameof( HelloWorld2Controller ), "2" )]
        [InlineData( "api/v3/helloworld/42", nameof( HelloWorld2Controller ), "3" )]
        public async Task then_get_with_id_should_return_200( string requestUrl, string controllerName, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "", id = "" };

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "2.0, 3.0, 4.0" );
            response.Headers.GetValues( "api-deprecated-versions" ).Single().Should().Be( "1.0" );
            content.Should().BeEquivalentTo( new { controller = controllerName, version = apiVersion, id = "42" } );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unsupported_version()
        {
            // arrange


            // act
            var response = await GetAsync( "api/v4/helloworld" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        public when_using_a_url_segment( ConventionsFixture fixture ) : base( fixture ) { }
    }
}