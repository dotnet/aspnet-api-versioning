namespace given_a_versioned_Controller_using_conventions
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Conventions;
    using Microsoft.AspNetCore.Mvc.Conventions.Controllers;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Collection( nameof( ConventionsCollection ) )]
    public class when_using_a_query_string_and_split_into_two_types : AcceptanceTest
    {
        [Theory]
        [InlineData( nameof( ValuesController ), "1.0" )]
        [InlineData( nameof( Values2Controller ), "2.0" )]
        [InlineData( nameof( Values2Controller ), "3.0" )]
        public async Task then_get_should_return_200( string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "" };

            // act
            var response = await GetAsync( $"api/values?api-version={apiVersion}" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
            content.Should().BeEquivalentTo( new { controller, version = apiVersion } );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unsupported_version()
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
        public async Task then_get_should_return_400_for_an_unspecified_version()
        {
            // arrange


            // act
            var response = await GetAsync( "api/values" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "ApiVersionUnspecified" );
        }

        public when_using_a_query_string_and_split_into_two_types( ConventionsFixture fixture ) : base( fixture ) { }
    }

    [Collection( nameof( ConventionsEndpointCollection ) )]
    public class when_using_a_query_string_and_split_into_two_types_ : when_using_a_query_string_and_split_into_two_types
    {
        public when_using_a_query_string_and_split_into_two_types_( ConventionsEndpointFixture fixture ) : base( fixture ) { }
    }
}