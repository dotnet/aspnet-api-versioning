namespace given
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.OData.Conventions;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class _a_query_string_versioned_ODataController_using_conventions : ConventionsAcceptanceTest
    {
        [Theory]
        [InlineData( "api/orders?api-version=1.0" )]
        [InlineData( "api/orders(42)?api-version=1.0" )]
        public async Task _get_should_return_200( string requestUrl )
        {
            // arrange


            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0" );
        }

        [Fact]
        public async Task _get_should_return_400_when_version_is_unsupported()
        {
            // arrange


            // act
            var response = await GetAsync( "api/orders?api-version=2.0" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        [Fact]
        public async Task _get_should_return_400_when_version_is_unspecified()
        {
            // arrange


            // act
            var response = await GetAsync( "api/orders" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "ApiVersionUnspecified" );
        }
    }
}