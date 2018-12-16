namespace Microsoft.AspNetCore.OData
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Trait( "Framework", "OData" )]
    public abstract class ODataAcceptanceTest : AcceptanceTest
    {
        [Fact]
        public async Task then_the_service_document_should_allow_an_unspecified_version()
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
        public async Task then_the_service_document_should_be_versionX2Dspecific( string apiVersion )
        {
            // arrange
            var requestUrl = $"api?api-version={apiVersion}";

            // act
            var response = await Client.GetAsync( requestUrl );

            // assert
            response.StatusCode.Should().Be( OK );
        }

        [Fact]
        public async Task then_the_service_document_should_return_400_for_an_unsupported_version()
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
        public async Task then_X24metadata_should_allow_an_unspecified_version()
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
        public async Task then_X24metadata_should_be_versionX2Dspecific( string apiVersion )
        {
            // arrange
            var requestUrl = $"api/$metadata?api-version={apiVersion}";

            // act
            var response = await Client.GetAsync( requestUrl );

            // assert
            response.StatusCode.Should().Be( OK );
        }

        [Fact]
        public async Task then_X24metadata_should_return_400_for_an_unsupported_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api/$metadata?api-version=4.0" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        protected ODataAcceptanceTest( ODataFixture fixture ) : base( fixture ) { }
    }
}