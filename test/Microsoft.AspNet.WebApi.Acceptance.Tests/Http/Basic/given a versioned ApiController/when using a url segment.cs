namespace given_a_versioned_ApiController
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.Http.Basic;
    using Microsoft.Web.Http.Basic.Controllers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Collection( nameof( BasicCollection ) )]
    public class when_using_a_url_segment : AcceptanceTest
    {
        [Theory]
        [InlineData( "api/v1/helloworld", null )]
        [InlineData( "api/v1/helloworld/42", "42" )]
        public async Task then_get_should_return_200( string requestUrl, string id )
        {
            // arrange
            var body = new Dictionary<string, string>()
            {
                ["controller"] = nameof( HelloWorldController ),
                ["version"] = "1"
            };

            if ( !string.IsNullOrEmpty( id ) )
            {
                body["id"] = id;
            }

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsAsync<IDictionary<string, string>>();

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0" );
            content.Should().BeEquivalentTo( body );
        }

        [Fact]
        public async Task then_post_should_return_201()
        {
            // arrange
            var entity = default( object );

            // act
            var response = await PostAsync( "api/v1/helloworld", entity );

            // assert
            response.StatusCode.Should().Be( Created );
            response.Headers.Location.Should().Be( new Uri( "http://localhost/api/v1/helloworld/42" ) );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unsupported_version()
        {
            // arrange


            // act
            var response = await GetAsync( "api/v2/helloworld" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        public when_using_a_url_segment( BasicFixture fixture ) : base( fixture ) { }
    }
}