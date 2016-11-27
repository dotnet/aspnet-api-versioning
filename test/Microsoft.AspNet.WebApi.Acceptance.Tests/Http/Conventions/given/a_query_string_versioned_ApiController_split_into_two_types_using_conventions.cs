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

    public class _a_query_string_versioned_ApiController_split_into_two_types_using_conventions : ConventionsAcceptanceTest
    {
        [Theory]
        [InlineData( nameof( ValuesController ), "1.0" )]
        [InlineData( nameof( Values2Controller ), "2.0" )]
        [InlineData( nameof( Values2Controller ), "3.0" )]
        public async Task _get_should_return_200( string controller, string apiVersion )
        {
            // arrange


            // act
            var response = await GetAsync( $"api/values?api-version={apiVersion}" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsAsync<IDictionary<string, string>>();

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
            content.ShouldBeEquivalentTo(
                new Dictionary<string, string>()
                {
                    ["controller"] = controller,
                    ["version"] = apiVersion
                } );
        }

        [Fact]
        public async Task _get_should_return_400_when_version_is_unsupported()
        {
            // arrange


            // act
            var response = await GetAsync( "api/values?api-version=4.0" );
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
            var response = await GetAsync( "api/values" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "ApiVersionUnspecified" );
        }
    }
}