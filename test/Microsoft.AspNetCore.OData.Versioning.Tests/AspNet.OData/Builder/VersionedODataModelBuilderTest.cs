namespace Microsoft.AspNet.OData.Builder
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.OData.Edm;
    using Moq;
    using System;
    using System.Linq;
    using Xunit;

    public class VersionedODataModelBuilderTest
    {
        [Fact]
        public void get_edm_models_should_return_expected_results()
        {
            // arrange
            var apiVersionProvider = new Mock<IODataApiVersionProvider>();
            var apiVersion = new ApiVersion( 1, 0 );

            apiVersionProvider.SetupGet( p => p.SupportedApiVersions ).Returns( new[] { apiVersion } );
            apiVersionProvider.SetupGet( p => p.DeprecatedApiVersions ).Returns( Array.Empty<ApiVersion>() );

            var defaultConfiguration = new Mock<Action<ODataModelBuilder, ApiVersion>>();
            var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
            var builder = new VersionedODataModelBuilder( apiVersionProvider.Object )
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