namespace Microsoft.AspNet.OData.Routing
{
    using FluentAssertions;
    using Microsoft.OData.Edm;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using Microsoft.AspNet.OData;
    using Xunit;
    using Microsoft.Web.Http;
    using Microsoft.AspNet.OData.Builder;

    public class VersionedAttributeRoutingConventionTest
    {
        [ApiVersionNeutral]
        sealed class NeutralController : ODataController { }

        [ApiVersion( "1.0" )]
        sealed class ControllerV1 : ODataController { }

        static IEdmModel CreateModel( HttpConfiguration configuration, Type controllerType )
        {
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new List<Type>() { controllerType };

            controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );

            var builder = new VersionedODataModelBuilder( configuration );

            return builder.GetEdmModels().Single();
        }

        [Fact]
        public void should_map_controller_should_return_true_for_versionX2Dneutral_controller()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var controller = new HttpControllerDescriptor( configuration, string.Empty, typeof( NeutralController ) );
            var convention = new VersionedAttributeRoutingConvention( "Tests", configuration, new ApiVersion( 1, 0 ) );

            // act
            var result = convention.ShouldMapController( controller );

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( 1, true )]
        [InlineData( 2, false )]
        public void should_map_controller_should_return_expected_result_for_controller_version( int majorVersion, bool expected )
        {
            // arrange
            var configuration = new HttpConfiguration();
            var controller = new HttpControllerDescriptor( configuration, string.Empty, typeof( ControllerV1 ) );
            var convention = new VersionedAttributeRoutingConvention( "Tests", configuration, new ApiVersion( majorVersion, 0 ) );

            // act
            var result = convention.ShouldMapController( controller );

            // assert
            result.Should().Be( expected );
        }
    }
}