namespace given_a_versioned_Controller
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Basic;
    using Microsoft.AspNetCore.Mvc.Basic.Controllers;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Collection( nameof( BasicCollection ) )]
    public class when_using_a_query_string_and_split_into_two_types : AcceptanceTest
    {
        [Theory]
        [InlineData( nameof( ValuesController ), "1.0" )]
        [InlineData( nameof( Values2Controller ), "2.0" )]
        public async Task then_get_should_return_200( string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", version = "" };

            // act
            var response = await GetAsync( $"api/values?api-version={apiVersion}" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
            content.Should().BeEquivalentTo( new { controller, version = apiVersion } );
        }

        [Fact]
        public async Task then_get_with_string_id_should_return_200()
        {
            // arrange
            var example = new { controller = "", id = "", version = "" };

            // act
            var response = await GetAsync( $"api/values/42?api-version=1.0" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
            content.Should().BeEquivalentTo( new { controller = nameof( ValuesController ), id = "42", version = "1.0" } );
        }

        [Fact]
        public async Task then_get_with_integer_id_should_return_200()
        {
            // arrange
            var example = new { controller = "", id = 0, version = "" };

            // act
            var response = await GetAsync( $"api/values/42?api-version=2.0" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
            content.Should().BeEquivalentTo( new { controller = nameof( Values2Controller ), id = 42, version = "2.0" } );
        }

        [Fact]
        public async Task then_get_returns_400_or_405_with_invalid_id()
        {
            // arrange
            var requestUrl = "api/values/abc?api-version=2.0";
            var statusCode = UsingEndpointRouting ? NotFound : BadRequest;

            // act
            var response = await GetAsync( requestUrl );

            // assert
            response.StatusCode.Should().Be( statusCode );
        }

        [Theory]
        [InlineData( "1.0" )]
        [InlineData( "2.0" )]
        public async Task then_delete_should_return_405( string apiVersion )
        {
            // arrange
            var requestUrl = $"api/values/42?api-version={apiVersion}";

            // act
            var response = await DeleteAsync( requestUrl );

            // assert
            response.StatusCode.Should().Be( MethodNotAllowed );
        }

        [Fact]
        public async Task then_get_should_return_400_for_an_unsupported_version()
        {
            // arrange


            // act
            var response = await GetAsync( "api/values?api-version=3.0" );
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

        [Theory]
        [InlineData( nameof( ValuesController ), "1.0" )]
        [InlineData( nameof( Values2Controller ), "2.0" )]
        public async Task then_action_segment_should_not_be_ambiguous_with_route_parameter( string controller, string apiVersion )
        {
            // arrange
            var example = new { controller = "", query = "", version = "" };

            // act
            var response = await GetAsync( $"api/values/search?query=Foo&api-version={apiVersion}" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0" );
            content.Should().BeEquivalentTo( new { controller, query = "Foo", version = apiVersion } );
        }

        public when_using_a_query_string_and_split_into_two_types( BasicFixture fixture ) : base( fixture ) { }
    }

    [Collection( nameof( BasicEndpointCollection ) )]
    public class when_using_a_query_string_and_split_into_two_types_ : when_using_a_query_string_and_split_into_two_types
    {
        public when_using_a_query_string_and_split_into_two_types_( BasicEndpointFixture fixture ) : base( fixture ) { }
    }
}