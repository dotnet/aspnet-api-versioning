namespace given_a_versioned_Controller
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.MediaTypeNegotiation;
    using Microsoft.AspNetCore.Mvc.MediaTypeNegotiation.Controllers;
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.Http.Headers.MediaTypeWithQualityHeaderValue;
    using static System.Net.HttpStatusCode;
    using static System.Text.Encoding;

    public class when_using_media_type_negotiation : AcceptanceTest, IClassFixture<MediaTypeNegotiationFixture>
    {
        [Theory]
        [InlineData( nameof( ValuesController ), "1.0" )]
        [InlineData( nameof( Values2Controller ), "2.0" )]
        public async Task then_get_should_return_200( string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "" };

            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.Accept.Add( Parse( "application/json;v=" + apiVersion ) );

            // act
            var response = await GetAsync( "api/values" ).EnsureSuccessStatusCode();
            var body = response.Content;
            var content = await body.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
            body.Headers.ContentType.Parameters.Single( p => p.Name == "v" ).Value.Should().Be( apiVersion );
            content.Should().BeEquivalentTo( new { controller, version = apiVersion } );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unsupported_version()
        {
            // arrange
            Client.DefaultRequestHeaders.Clear();
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
        public async Task then_get_should_return_current_version_for_an_unspecified_version( string requestUrl, string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "" };

            Client.DefaultRequestHeaders.Clear();

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var body = response.Content;
            var content = await body.ReadAsExampleAsync( example );

            // assert
            body.Headers.ContentType.Parameters.Single( p => p.Name == "v" ).Value.Should().Be( apiVersion );
            content.Should().BeEquivalentTo( new { controller, version = apiVersion } );
        }

        [Fact]
        public async Task then_post_should_return_201()
        {
            // arrange
            var content = new StringContent( "{\"text\":\"Test\"}", UTF8 );

            Client.DefaultRequestHeaders.Clear();
            content.Headers.ContentType = Parse( "application/json;v=1.0" );

            // act
            var response = await PostAsync( "api/helloworld", content );

            // assert
            response.StatusCode.Should().Be( Created );
            response.Headers.Location.Should().Be( new Uri( "http://localhost/api/HelloWorld/42" ) );
        }

        public when_using_media_type_negotiation( MediaTypeNegotiationFixture fixture ) : base( fixture ) { }
    }

    public class when_using_media_type_negotiation_ : when_using_media_type_negotiation, IClassFixture<MediaTypeNegotiationEndpointFixture>
    {
        public when_using_media_type_negotiation_( MediaTypeNegotiationEndpointFixture fixture ) : base( fixture ) { }
    }
}