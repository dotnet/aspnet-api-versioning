namespace given_a_versioned_ApiController
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.Http.MediaTypeNegotiation;
    using Microsoft.Web.Http.MediaTypeNegotiation.Controllers;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.Http.Headers.MediaTypeWithQualityHeaderValue;
    using static System.Net.HttpStatusCode;

    public class when_using_media_type_negotiation : MediaTypeNegotiationAcceptanceTest
    {
        [Theory]
        [InlineData( nameof( ValuesController ), "1.0" )]
        [InlineData( nameof( Values2Controller ), "2.0" )]
        public async Task then_get_should_return_200( string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "" };

            Client.DefaultRequestHeaders.Accept.Add( Parse( "application/json;v=" + apiVersion ) );

            // act
            var response = await GetAsync( "api/values" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
            content.ShouldBeEquivalentTo( new { controller = controller, version = apiVersion } );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unsupported_version()
        {
            // arrange
            Client.DefaultRequestHeaders.Accept.Add( Parse( "application/json;v=3.0" ) );

            // act
            var response = await GetAsync( "api/values" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        [Theory]
        [InlineData( "api/values", nameof( Values2Controller ), "2.0" )]
        [InlineData( "api/helloworld", nameof( HelloWorldController ), "1.0" )]
        public async Task then_get_should_allow_an_unspecified_version( string requestUrl, string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "" };

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            content.ShouldBeEquivalentTo( new { controller = controller, version = apiVersion } );
        }

        [Fact]
        public async Task then_post_should_return_201()
        {
            // arrange
            var entity = new { text = "Test" };
            var mediaType = Parse( "application/json;v=1.0" );
            var content = new ObjectContent( entity.GetType(), entity, new JsonMediaTypeFormatter(), mediaType );

            // act
            var response = await PostAsync( "api/helloworld", content ).EnsureSuccessStatusCode();

            // assert
            response.Headers.Location.Should().Be( new Uri( "http://localhost/api/helloworld/42" ) );
        }
    }
}