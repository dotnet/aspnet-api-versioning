namespace Microsoft.Web.OData.Builder
{
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
    using System.Web.OData.Builder;
    using Xunit;

    public class VersionedODataModelBuilderTest
    {
        [Fact]
        public void get_edm_models_should_return_expected_results()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var controllerDescriptor = new HttpControllerDescriptor( configuration, "Test", typeof( IHttpController ) );
            var controllerMapping = new Dictionary<string, HttpControllerDescriptor>() { { "Test", controllerDescriptor } };
            var controllerSelector = new Mock<IHttpControllerSelector>();
            var defaultConfiguration = new Mock<Action<ODataModelBuilder, ApiVersion>>();
            var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
            var apiVersion = new ApiVersion( 1, 0 );
            var builder = new VersionedODataModelBuilder( controllerSelector.Object )
            {
                DefaultModelConfiguration = defaultConfiguration.Object,
                OnModelCreated = modelCreated.Object
            };

            controllerSelector.Setup( cs => cs.GetControllerMapping() ).Returns( controllerMapping );

            // act
            var model = builder.GetEdmModels().Single();

            // assert
            model.GetAnnotationValue<ApiVersionAnnotation>( model ).ApiVersion.Should().Be( apiVersion );
            defaultConfiguration.Verify( f => f( It.IsAny<ODataModelBuilder>(), apiVersion ), Times.Once() );
            modelCreated.Verify( f => f( It.IsAny<ODataModelBuilder>(), model ), Times.Once() );
        }
    }
}
