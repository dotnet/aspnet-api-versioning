namespace Microsoft.Web.OData
{
    using FluentAssertions;
    using Microsoft.OData.UriParser;
    using OData.Controllers;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    [Trait( "Framework", "OData" )]
    public abstract class ODataAcceptanceTest : AcceptanceTest
    {
        protected ODataAcceptanceTest() => FilteredControllerTypes.Add( typeof( VersionedMetadataController ) );

        protected ODataUriResolver TestUriResolver { get; } = new CustomUriResolver();

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

        // HACK: required due to bug in ODL
        // REF: https://github.com/OData/odata.net/issues/695
        sealed class CustomUriResolver : UnqualifiedODataUriResolver
        {
            public override bool EnableCaseInsensitive { get => true; set { } }
        }
    }
}