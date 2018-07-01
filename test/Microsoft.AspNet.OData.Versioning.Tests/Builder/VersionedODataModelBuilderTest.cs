namespace Microsoft.AspNet.OData.Builder
{
    using FluentAssertions;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using Xunit;

    public class VersionedODataModelBuilderTest
    {
        [ApiVersion( "1.0" )]
        sealed class ControllerV1 : ODataController { }

        [Fact]
        public void get_edm_models_should_return_expected_results()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new List<Type>() { typeof( ControllerV1 ) };

            controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );

            var defaultConfiguration = new Mock<Action<ODataModelBuilder, ApiVersion>>();
            var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
            var apiVersion = new ApiVersion( 1, 0 );
            var builder = new VersionedODataModelBuilder( configuration )
            {
                DefaultModelConfiguration = defaultConfiguration.Object,
                OnModelCreated = modelCreated.Object
            };

            // act
            var model = builder.GetEdmModels().Single();

            // assert
            model.GetAnnotationValue<ApiVersionAnnotation>( model ).ApiVersion.Should().Be( apiVersion );
            defaultConfiguration.Verify( f => f( It.IsAny<ODataModelBuilder>(), apiVersion ), Times.Once() );
            modelCreated.Verify( f => f( It.IsAny<ODataModelBuilder>(), model ), Times.Once() );
        }
    }
}