namespace Microsoft.AspNet.OData.Builder
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Options;
    using Microsoft.OData.Edm;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit;
    using static Moq.Times;

    public class VersionedODataModelBuilderTest
    {
        [Fact]
        public void get_edm_models_should_return_expected_results()
        {
            // arrange
            var actionDescriptorCollectionProvider = NewActionDescriptorCollectionProvider();
            var apiVersion = new ApiVersion( 1, 0 );
            var options = new OptionsWrapper<ApiVersioningOptions>( new ApiVersioningOptions() { DefaultApiVersion = apiVersion } );
            var defaultConfiguration = new Mock<Action<ODataModelBuilder, ApiVersion>>();
            var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
            var builder = new VersionedODataModelBuilder( actionDescriptorCollectionProvider, options )
            {
                DefaultModelConfiguration = defaultConfiguration.Object,
                OnModelCreated = modelCreated.Object
            };

            // act
            var model = builder.GetEdmModels().Single();

            // assert
            model.GetAnnotationValue<ApiVersionAnnotation>( model ).ApiVersion.Should().Be( apiVersion );
            defaultConfiguration.Verify( f => f( It.IsAny<ODataModelBuilder>(), apiVersion ), Once() );
            modelCreated.Verify( f => f( It.IsAny<ODataModelBuilder>(), model ), Once() );
        }

        static IActionDescriptorCollectionProvider NewActionDescriptorCollectionProvider()
        {
            var provider = new Mock<IActionDescriptorCollectionProvider>();
            var items = new[]
            {
                new ControllerActionDescriptor()
                {
                    ControllerTypeInfo = typeof( ODataController ).GetTypeInfo(),
                    Properties = new Dictionary<object, object>()
                    {
                        [typeof( ApiVersionModel )] = new ApiVersionModel( new ApiVersion( 1, 0 ) ),
                    },
                },
            };
            var collection = new ActionDescriptorCollection( items, 0 );

            provider.SetupGet( p => p.ActionDescriptors ).Returns( collection );

            return provider.Object;
        }
    }
}