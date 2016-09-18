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
        [InlineData( "api/v1/helloworld", "1", null )]
        [InlineData( "api/v2/helloworld", "2", null )]
        [InlineData( "api/v1/helloworld/42", "1", "42" )]
        [InlineData( "api/v2/helloworld/42", "2", "42" )]
        public async Task _get_should_return_200( string requestUrl, string apiVersion, string id )
        {
            // arrange
            var body = new Dictionary<string, string>()
            {
                ["controller"] = nameof( HelloWorldController ),
                ["version"] = apiVersion
            };

            if ( !string.IsNullOrEmpty( id ) )
            {
                body["id"] = id;
            }

            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsAsync<IDictionary<string, string>>();

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
            content.ShouldBeEquivalentTo( body );
        }

        [Fact]
        public async Task _get_should_return_400_when_version_is_unsupported()
        {
            // arrange


            // act
            var response = await GetAsync( "api/v3/helloworld" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}