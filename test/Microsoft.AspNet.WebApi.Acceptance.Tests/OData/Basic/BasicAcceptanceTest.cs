namespace Microsoft.Web.OData.Basic
{
    using Builder;
    using Configuration;
    using Controllers;
    using FluentAssertions;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.OData.Builder;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public abstract class BasicAcceptanceTest : ODataAcceptanceTest
    {
        protected BasicAcceptanceTest()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ) );
            FilteredControllerTypes.Add( typeof( PeopleController ) );
            FilteredControllerTypes.Add( typeof( People2Controller ) );

            Configuration.AddApiVersioning( options => options.ReportApiVersions = true );

            var modelBuilder = new VersionedODataModelBuilder( Configuration )
            {
                ModelBuilderFactory = () => new ODataConventionModelBuilder().EnableLowerCamelCase(),
                ModelConfigurations =
                {
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration()
                }
            };
            var models = modelBuilder.GetEdmModels();

            Configuration.MapVersionedODataRoutes( "odata", "api", models );
            Configuration.MapVersionedODataRoutes( "odata-bypath", "v{apiVersion}", models );
            Configuration.EnsureInitialized();
        }

        [Fact]
        public async Task _service_document_should_return_400_for_unsupported_url_api_version()
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
        public async Task _metadata_should_return_400_for_unsupported_url_api_version()
        {
            // arrange

            // act
            var response = await Client.GetAsync( "v4/$metadata" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}