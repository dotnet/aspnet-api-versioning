namespace Microsoft.Web.OData
{
    using FluentAssertions;
    using OData.Controllers;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.OData.Extensions;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Trait( "Framework", "OData" )]
    public abstract class ODataAcceptanceTest : AcceptanceTest
    {
        protected ODataAcceptanceTest()
        {
            FilteredControllerTypes.Add( typeof( VersionedMetadataController ) );
            Configuration.EnableCaseInsensitive( true );
            Configuration.EnableUnqualifiedNameCall( true );
        }

        [Fact]
        public async Task _service_document_should_return_result_without_api_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api" );

            // assert
            response.StatusCode.Should().Be( OK );
        }

        [Theory]
        [InlineData( "1.0" )]
        [InlineData( "2.0" )]
        [InlineData( "3.0" )]
        public async Task _service_document_should_return_api_version_specific_result( string apiVersion )
        {
            // arrange
            var requestUrl = $"api?api-version={apiVersion}";

            // act
            var response = await Client.GetAsync( requestUrl );

            // assert
            response.StatusCode.Should().Be( OK );
        }

        [Fact]
        public async Task _service_document_should_return_400_for_unsupported_query_string_api_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api?api-version=4.0" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        [Fact]
        public async Task _metadata_should_return_result_without_api_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/$metadata" );

            // assert
            response.StatusCode.Should().Be( OK );
        }

        [Theory]
        [InlineData( "1.0" )]
        [InlineData( "2.0" )]
        [InlineData( "3.0" )]
        public async Task _metadata_should_return_api_version_specific_result( string apiVersion )
        {
            // arrange
            var requestUrl = $"api/$metadata?api-version={apiVersion}";

            // act
            var response = await Client.GetAsync( requestUrl );

            // assert
            response.StatusCode.Should().Be( OK );
        }

        [Fact]
        public async Task _metadata_should_return_400_for_unsupported_query_string_api_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/$metadata?api-version=4.0" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}