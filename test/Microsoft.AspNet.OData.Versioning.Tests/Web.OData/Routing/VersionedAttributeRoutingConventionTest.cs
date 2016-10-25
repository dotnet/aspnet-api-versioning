namespace Microsoft.Web.OData.Routing
{
    using Builder;
    using FluentAssertions;
    using Http;
    using Microsoft.OData.Edm;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dispatcher;
    using System.Web.OData;
    using System.Web.OData.Builder;
    using Xunit;

    public class VersionedAttributeRoutingConventionTest
    {
        [ApiVersionNeutral]
        private sealed class NeutralController : ODataController
        {
        }

        [ApiVersion( "1.0" )]
        private sealed class ControllerV1 : ODataController
        {
        }

        private static IEdmModel CreateModel( HttpConfiguration configuration, Type controllerType )
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
            var model = new ODataModelBuilder().GetEdmModel();
            var controller = new HttpControllerDescriptor( new HttpConfiguration(), string.Empty, typeof( NeutralController ) );
            var convention = new VersionedAttributeRoutingConvention( model, new HttpControllerDescriptor[0] );
            var annotation = new ApiVersionAnnotation( new ApiVersion( 1, 0 ) );

            model.SetAnnotationValue( model, annotation );

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
            var model = new ODataModelBuilder().GetEdmModel();
            var controller = new HttpControllerDescriptor( new HttpConfiguration(), string.Empty, typeof( ControllerV1 ) );
            var convention = new VersionedAttributeRoutingConvention( model, new HttpControllerDescriptor[0] );
            var annotation = new ApiVersionAnnotation( new ApiVersion( majorVersion, 0 ) );

            model.SetAnnotationValue( model, annotation );

            // act
            var result = convention.ShouldMapController( controller );

            // assert
            result.Should().Be( expected );
        }
    }
}