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
            var apiVersion = new ApiVersion( 1, 0 );
            var actionDescriptorCollectionProvider = NewActionDescriptorCollectionProvider( new[] { apiVersion } );
            var options = Options.Create( new ApiVersioningOptions() { DefaultApiVersion = apiVersion } );
            var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
            var builder = new VersionedODataModelBuilder( actionDescriptorCollectionProvider, options )
            {
                DefaultModelConfiguration = ( b, v, r ) => b.EntitySet<TestEntity>( "Tests" ),
                OnModelCreated = modelCreated.Object
            };

            // act
            var model = builder.GetEdmModels().Single();

            // assert
            model.GetAnnotationValue<ApiVersionAnnotation>( model ).ApiVersion.Should().Be( apiVersion );
            modelCreated.Verify( f => f( It.IsAny<ODataModelBuilder>(), model ), Once() );
        }

        [Fact]
        public void get_edm_models_should_split_models_between_routes()
        {
            // arrange
            var apiVersion = new ApiVersion( 1, 0 );
            var actionDescriptorCollectionProvider = NewActionDescriptorCollectionProvider( new[] { apiVersion, new ApiVersion( 2, 0 ) } );
            var options = Options.Create( new ApiVersioningOptions() { DefaultApiVersion = apiVersion } );
            var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
            var builder = new VersionedODataModelBuilder( actionDescriptorCollectionProvider, options )
            {
                DefaultModelConfiguration = ( builder, version, prefix ) =>
                {
                    if ( prefix == "api2" )
                    {
                        builder.EntitySet<TestEntity>( "Tests" );
                    }
                },
            };

            // act
            var models = builder.GetEdmModels( "api2" );

            // assert
            models.Should().HaveCount( 2 );
            models.ElementAt( 1 ).FindDeclaredEntitySet( "Tests" ).Should().NotBeNull();
        }

        static IActionDescriptorCollectionProvider NewActionDescriptorCollectionProvider( IReadOnlyList<ApiVersion> versions )
        {
            var provider = new Mock<IActionDescriptorCollectionProvider>();
            var items = new List<ControllerActionDescriptor>( capacity: versions.Count );

            for ( var i = 0; i < versions.Count; i++ )
            {
                items.Add(
                    new ControllerActionDescriptor()
                    {
                        ControllerTypeInfo = typeof( ODataController ).GetTypeInfo(),
                        Properties = new Dictionary<object, object>()
                        {
                            [typeof( ApiVersionModel )] = new ApiVersionModel( versions[i] ),
                        },
                    } );
            }
            var collection = new ActionDescriptorCollection( items, 0 );

            provider.SetupGet( p => p.ActionDescriptors ).Returns( collection );

            return provider.Object;
        }
    }
}