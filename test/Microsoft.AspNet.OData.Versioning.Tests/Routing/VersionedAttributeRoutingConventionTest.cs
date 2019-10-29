namespace Microsoft.AspNet.OData.Routing
{
    using FluentAssertions;
    using Microsoft.AspNet.OData;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Xunit;

    public class VersionedAttributeRoutingConventionTest
    {
        [ApiVersionNeutral]
        sealed class NeutralController : ODataController { }

        [ApiVersion( "1.0" )]
        sealed class ControllerV1 : ODataController { }

        [Fact]
        public void should_map_controller_should_return_true_for_versionX2Dneutral_controller()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var controller = new HttpControllerDescriptor( configuration, string.Empty, typeof( NeutralController ) );
            var convention = new VersionedAttributeRoutingConvention( "Tests", configuration, new ApiVersion( 1, 0 ) );

            controller.Properties[typeof( ApiVersionModel )] = ApiVersionModel.Neutral;

            // act
            var result = convention.ShouldMapController( controller );

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( 1 )]
        [InlineData( 2 )]
        public void should_map_controller_should_return_expected_result_for_controller_version( int majorVersion )
        {
            // arrange
            var configuration = new HttpConfiguration();
            var controller = new HttpControllerDescriptor( configuration, string.Empty, typeof( ControllerV1 ) );
            var convention = new VersionedAttributeRoutingConvention( "Tests", configuration, new ApiVersion( majorVersion, 0 ) );

            controller.Properties[typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) );

            // act
            var result = convention.ShouldMapController( controller );

            // assert
            result.Should().BeTrue();
        }
    }
}