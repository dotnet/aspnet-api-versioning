namespace given
{
    using FluentAssertions;
    using Microsoft.Web;
    using Microsoft.Web.Http.ByNamespace;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.HttpStatusCode;

    public class _a_url_versioned_ApiController_per_namespace : ByNamespaceAcceptanceTest
    {
        public _a_url_versioned_ApiController_per_namespace() : base( SetupKind.Agreements ) { }

        [Theory]
        [InlineData( "Microsoft.Web.Http.ByNamespace.Controllers.V1.AgreementsController", "1" )]
        [InlineData( "Microsoft.Web.Http.ByNamespace.Controllers.V2.AgreementsController", "2" )]
        [InlineData( "Microsoft.Web.Http.ByNamespace.Controllers.V3.AgreementsController", "3" )]
        public async Task _get_should_return_200( string controller, string apiVersion )
        {
            // arrange
            var example = new { Controller = "", ApiVersion = "", AccountId = "" };

            // act
            var response = await GetAsync( $"v{apiVersion}/agreements/42" ).EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsExampleAsync( example );

            // assert
            response.Headers.GetValues( "api-supported-versions" ).Single().Should().Be( "1.0, 2.0, 3.0" );
            content.ShouldBeEquivalentTo( new { Controller = controller, ApiVersion = apiVersion, AccountId = "42" } );
        }

        [Fact]
        public async Task _get_should_return_400_when_version_is_unsupported()
        {
            // arrange


            // act
            var response = await GetAsync( "v4/agreements/42" );
            var content = await response.Content.ReadAsAsync<OneApiErrorResponse>();

            // assert
            response.StatusCode.Should().Be( BadRequest );
            content.Error.Code.Should().Be( "UnsupportedApiVersion" );
        }
    }
}