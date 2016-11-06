namespace given
{
    using FluentAssertions;
    using Microsoft.Web.Http.ByNamespace;
    using System.Threading.Tasks;
    using Xunit;

    public class _a_url_versioned_and_attributeX2Drouted_ApiController_per_namespace : ByNamespaceAcceptanceTest
    {
        public _a_url_versioned_and_attributeX2Drouted_ApiController_per_namespace() : base( SetupKind.HelloWorld ) { }

        [Fact]
        public async Task _then_get_should_should_return_200_when_a_version_is_unspecified()
        {
            // arrange


            // act
            var response = await GetAsync( "api/helloworld" );
            var content = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // assert
            content.Should().Be( "\"V2\"" );
        }

        [Theory]
        [InlineData( "Microsoft.Web.Http.ByNamespace.Controllers.V1.HelloWorldController", "1" )]
        [InlineData( "Microsoft.Web.Http.ByNamespace.Controllers.V2.HelloWorldController", "2" )]
        [InlineData( "Microsoft.Web.Http.ByNamespace.Controllers.V3.HelloWorldController", "3" )]
        public async Task _get_should_return_200( string controller, string apiVersion )
        {
            // arrange
            var expected = $"\"V{apiVersion}\"";

            // act
            var response = await GetAsync( $"api/{apiVersion}/helloworld" );
            var content = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // assert
            content.Should().Be( expected );
        }
    }
}