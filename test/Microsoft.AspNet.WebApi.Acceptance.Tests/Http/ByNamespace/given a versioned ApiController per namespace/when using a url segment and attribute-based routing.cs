namespace given_a_versioned_ApiController_per_namespace
{
    using FluentAssertions;
    using Microsoft.Web.Http.ByNamespace;
    using System.Threading.Tasks;
    using Xunit;

    public class when_using_a_url_segment_and_attributeX2Dbased_routing : ByNamespaceAcceptanceTest
    {
        public when_using_a_url_segment_and_attributeX2Dbased_routing() : base( SetupKind.HelloWorld ) { }

        [Fact]
        public async Task then_get_should_should_return_200_for_an_unspecified_version()
        {
            // arrange


            // act
            var response = await GetAsync( "api/helloworld" );
            var content = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync();

            // assert
            content.Should().Be( "\"V2\"" );
        }

        [Theory]
        [InlineData( "1" )]
        [InlineData( "2" )]
        [InlineData( "3" )]
        public async Task then_get_should_return_200( string apiVersion )
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