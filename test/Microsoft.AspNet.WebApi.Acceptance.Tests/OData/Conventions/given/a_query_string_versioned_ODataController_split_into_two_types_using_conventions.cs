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

    public class _a_query_string_versioned_ODataController_split_into_two_types_using_conventions : ConventionsAcceptanceTest
    {
        [Theory]
        [InlineData( "api/people?api-version=1.0" )]
        [InlineData( "api/people(42)?api-version=1.0" )]
        [InlineData( "api/people?api-version=2.0" )]
        [InlineData( "api/people(42)?api-version=2.0" )]
        [InlineData( "api/people?api-version=3.0" )]
        [InlineData( "api/people(42)?api-version=3.0" )]
        public async Task _get_should_return_200( string requestUrl )
        {
            // arrange


            // act
            var response = await GetAsync( requestUrl ).EnsureSuccessStatusCode();

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
        }

        [Fact]
        public async Task _get_should_return_400_when_version_is_unsupported()
        {
            // arrange


            // act
            var response = await GetAsync( "api/people?api-version=4.0" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        [Fact]
        public async Task _patch_should_return_204()
        {
            // arrange
            var person = new { id = 42, firstName = "John", lastName = "Doe", email = "john.doe@somewhere.com" };

            // act
            var response = await PatchAsync( "api/people(42)?api-version=2.0", person );

            // assert
            response.StatusCode.Should().Be( NoContent );
        }

        [Theory]
        [InlineData( "api/people(42)?api-version=1.0" )]
        [InlineData( "api/people(42)?api-version=3.0" )]
        [InlineData( "api/people(42)?api-version=4.0" )]
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