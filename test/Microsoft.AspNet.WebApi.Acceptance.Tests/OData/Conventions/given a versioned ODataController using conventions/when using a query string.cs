namespace given_a_versioned_ODataController_using_conventions
{
    using FluentAssertions;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Conventions;
    using Microsoft.Web;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Collection( nameof( ConventionsODataCollection ) )]
    public class when_using_a_query_string : ODataAcceptanceTest
    {
        [Theory]
        [InlineData( "api/orders?api-version=1.0" )]
        [InlineData( "api/orders(42)?api-version=1.0" )]
        public async Task then_get_should_return_200( string requestUrl )
        {
            // arrange


            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0" );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unsupported_version()
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
        public async Task then_get_should_return_400_for_an_unspecified_version()
        {
            // arrange


            // act
            var response = await GetAsync( "api/orders" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "ApiVersionUnspecified" );
        }

        public when_using_a_query_string( ConventionsFixture fixture ) : base( fixture ) { }
    }
}