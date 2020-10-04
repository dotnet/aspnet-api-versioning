﻿namespace Microsoft.AspNet.OData.Builder
{
    using FluentAssertions;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Dispatcher;
    using Microsoft.Web.Http.Versioning;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using Xunit;

    public class VersionedODataModelBuilderTest
    {
        [Fact]
        public void get_edm_models_should_return_expected_results()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new List<Type>() { typeof( TestController ) };

            controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );

            var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
            var apiVersion = new ApiVersion( 1, 0 );
            var builder = new VersionedODataModelBuilder( configuration )
            {
                DefaultModelConfiguration = ( builder, version, prefix ) => builder.EntitySet<TestEntity>( "Tests" ),
                OnModelCreated = modelCreated.Object
            };

            // act
            var model = builder.GetEdmModels().Single();

            // assert
            model.GetAnnotationValue<ApiVersionAnnotation>( model ).ApiVersion.Should().Be( apiVersion );
            modelCreated.Verify( f => f( It.IsAny<ODataModelBuilder>(), model ), Times.Once() );
        }

        [Fact]
        public void get_edm_models_should_split_models_between_routes()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new List<Type>() { typeof( TestController ), typeof( OtherTestController ) };
            var options = new ApiVersioningOptions();

            controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.Services.Replace( typeof( IHttpControllerSelector ), new ApiVersionControllerSelector( configuration, options ) );

            var defaultConfiguration = new Mock<Action<ODataModelBuilder, ApiVersion, string>>();
            var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
            var builder = new VersionedODataModelBuilder( configuration )
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
    }

    [ApiVersion( "1.0" )]
    public sealed class TestController : ODataController
    {
        public IHttpActionResult Get() => Ok();
    }

    [ApiVersion( "2.0" )]
    public sealed class OtherTestController : ODataController
    {
        public IHttpActionResult Get() => Ok();
    }
}