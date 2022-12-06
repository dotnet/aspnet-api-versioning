// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using static Moq.Times;

public class VersionedODataModelBuilderTest
{
    [Fact]
    public void get_edm_models_should_return_expected_results()
    {
        // arrange
        var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
        var apiVersion = new ApiVersion( 1, 0 );
        var apiVersionCollectionProvider = Mock.Of<IODataApiVersionCollectionProvider>();

        apiVersionCollectionProvider.ApiVersions = new[] { apiVersion };

        var modelConfigurations = Enumerable.Empty<IModelConfiguration>();
        var builder = new VersionedODataModelBuilder( apiVersionCollectionProvider, modelConfigurations )
        {
            DefaultModelConfiguration = ( b, v, r ) => b.EntitySet<TestEntity>( "Tests" ),
            OnModelCreated = modelCreated.Object,
        };

        // act
        var model = builder.GetEdmModels().Single();

        // assert
        model.GetApiVersion().Should().Be( apiVersion );
        modelCreated.Verify( f => f( It.IsAny<ODataModelBuilder>(), model ), Once() );
    }

    [Fact]
    public void get_edm_models_should_split_models_between_routes()
    {
        // arrange
        var modelCreated = new Mock<Action<ODataModelBuilder, IEdmModel>>();
        var apiVersion = new ApiVersion( 1, 0 );
        var apiVersionCollectionProvider = Mock.Of<IODataApiVersionCollectionProvider>();

        apiVersionCollectionProvider.ApiVersions = new[] { apiVersion, new ApiVersion( 2, 0 ) };

        var modelConfigurations = Enumerable.Empty<IModelConfiguration>();
        var builder = new VersionedODataModelBuilder( apiVersionCollectionProvider, modelConfigurations )
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