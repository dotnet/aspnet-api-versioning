namespace Microsoft.AspNetCore.OData.Basic
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Collection( nameof( BasicODataCollection ) )]
    public abstract class BasicAcceptanceTest : ODataAcceptanceTest
    {
        [Fact]
        public async Task then_service_document_should_return_400_for_unsupported_url_api_version()
        {
            // arrange
            var requestUrl = $"v4";

            // act
            var response = await Client.GetAsync( requestUrl );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        [Fact]
        public async Task then_X24metadata_should_return_400_for_unsupported_url_api_version()
        {
            // arrange

            // act
            var response = await Client.GetAsync( "v4/$metadata" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }

        [Fact]
        public async Task then_the_service_document_should_return_only_path_for_an_unsupported_version()
        {
            // arrange


            // act
            var response = await Client.GetAsync( "api?api-version=4.0" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
            content.Error.Message.Should().Contain( "api" );
            content.Error.Message.Should().NotContain( "?api-version=1.0" );
        }

        protected BasicAcceptanceTest( BasicFixture fixture ) : base( fixture ) { }
    }
}