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

    public class _a_url_versioned_ODataController_split_into_two_types_using_conventions : ConventionsAcceptanceTest
    {
        [Theory]
        [InlineData( "v1/people" )]
        [InlineData( "v1/people(42)" )]
        [InlineData( "v2/people" )]
        [InlineData( "v2/people(42)" )]
        [InlineData( "v3/people" )]
        [InlineData( "v3/people(42)" )]
        public async Task _get_should_return_200( string requestUrl )
        {
            // arrange


            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
        }

        [Fact]
        public async Task _patch_should_return_204()
        {
            // arrange
            var person = new { id = 42, firstName = "John", lastName = "Doe", email = "john.doe@somewhere.com" };

            // act
            var response = await PatchAsync( "v2/people(42)", person );

            // assert
            response.StatusCode.Should().Be( NoContent );
        }

        [Theory]
        [InlineData( "v1/people(42)" )]
        [InlineData( "v3/people(42)" )]
        [InlineData( "v4/people(42)" )]
        public async Task _patch_should_return_400_when_version_is_unsupported( string requestUrl )
        {
            // arrange
            var person = new { id = 42, firstName = "John", lastName = "Doe", email = "john.doe@somewhere.com" };

            // act
            var response = await PatchAsync( requestUrl, person );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}