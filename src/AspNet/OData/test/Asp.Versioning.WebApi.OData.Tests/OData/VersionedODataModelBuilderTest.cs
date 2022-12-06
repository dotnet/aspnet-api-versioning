// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using System.Web.Http;
using System.Web.Http.Dispatcher;

public class VersionedODataModelBuilderTest
{
    [Fact]
    public void get_edm_models_should_return_expected_results()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
        var controllerTypes = new List<Type>() { typeof( TestController ) };

        configuration.AddApiVersioning();
        controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
        configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );

        var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
        var apiVersion = new ApiVersion( 1, 0 );
        var builder = new VersionedODataModelBuilder( configuration )
        {
            DefaultModelConfiguration = ( builder, version, prefix ) => builder.EntitySet<TestEntity>( "Tests" ),
            OnModelCreated = modelCreated.Object,
        };

        // act
        var model = builder.GetEdmModels().Single();

        // assert
        model.GetApiVersion().Should().Be( apiVersion );
        modelCreated.Verify( f => f( It.IsAny<ODataModelBuilder>(), model ), Times.Once() );
    }

    [Fact]
    public void get_edm_models_should_split_models_between_routes()
    {
        // arrange
        var configuration = new HttpConfiguration();
        var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
        var controllerTypes = new List<Type>() { typeof( TestController ), typeof( OtherTestController ) };

        configuration.AddApiVersioning();
        controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
        configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );

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

#pragma warning disable SA1402 // File may only contain a single type

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