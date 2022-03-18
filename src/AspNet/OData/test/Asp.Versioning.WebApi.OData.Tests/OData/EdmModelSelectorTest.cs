// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData;

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using System.Net.Http;

public class EdmModelSelectorTest
{
    [Fact]
    public void new_edm_model_selector_should_use_model_annotations()
    {
        // arrange
        var model = NewEdm( new ApiVersion( 2.0 ) );

        // act
        var selector = new EdmModelSelector( new[] { model }, Mock.Of<IApiVersionSelector>() );

        // assert
        selector.ApiVersions.Single().Should().Be( new ApiVersion( 2.0 ) );
    }

    [Theory]
    [InlineData( 2.0, true )]
    [InlineData( 1.0, false )]
    public void contains_should_have_expected_api_version( double version, bool expected )
    {
        // arrange
        var model = NewEdm( new ApiVersion( 2.0 ) );

        // act
        var selector = new EdmModelSelector( new[] { model }, Mock.Of<IApiVersionSelector>() );

        // assert
        selector.Contains( new ApiVersion( version ) ).Should().Be( expected );
    }

    [Fact]
    public void select_model_return_matching_edm()
    {
        // arrange
        var model = NewEdm( new ApiVersion( 2.0 ) );
        var selector = new EdmModelSelector( new[] { model }, Mock.Of<IApiVersionSelector>() );

        // act
        var result = selector.SelectModel( new ApiVersion( 2.0 ) );

        // assert
        model.Should().BeSameAs( result );
    }

    [Fact]
    public void select_model_return_null_for_unmatched_api_version()
    {
        // arrange
        var model = NewEdm( new ApiVersion( 2.0 ) );
        var selector = new EdmModelSelector( new[] { model }, Mock.Of<IApiVersionSelector>() );

        // act
        var result = selector.SelectModel( new ApiVersion( 1.0 ) );

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public void select_model_should_return_newest_edm_without_request()
    {
        // arrange
        var model = NewEdm( new ApiVersion( 2.0 ) );
        var selector = new EdmModelSelector( new[] { model }, Mock.Of<IApiVersionSelector>() );

        // act
        var result = selector.SelectModel( Mock.Of<IServiceProvider>() );

        // assert
        result.Should().BeSameAs( model );
    }

    [Fact]
    public void select_model_should_return_edm_from_requested_api_version()
    {
        // arrange
        var model = NewEdm( new ApiVersion( 2.0 ) );
        var serviceProvider = NewServiceProvider( new ApiVersion( 2.0 ) );
        var selector = new EdmModelSelector(
            new[] { NewEdm( new ApiVersion( 1.0 ) ), model },
            Mock.Of<IApiVersionSelector>() );

        // act
        var result = selector.SelectModel( serviceProvider );

        // assert
        model.Should().BeSameAs( result );
    }

    [Fact]
    public void select_model_should_return_edm_from_selected_api_version()
    {
        // arrange
        var model = NewEdm( new ApiVersion( 1.0 ) );
        var serviceProvider = NewServiceProvider();
        var selector = new EdmModelSelector(
            new[] { NewEdm( new ApiVersion( 2.0 ) ), model },
            new LowestImplementedApiVersionSelector( new() ) );

        // act
        var result = selector.SelectModel( serviceProvider );

        // assert
        model.Should().BeSameAs( result );
    }

    private static IServiceProvider NewServiceProvider( ApiVersion apiVersion = default )
    {
        var request = new HttpRequestMessage();
        var properties = request.ApiVersionProperties();

        properties.RequestedApiVersion = apiVersion;

        var serviceProvider = new Mock<IServiceProvider>();

        serviceProvider.Setup( sp => sp.GetService( typeof( HttpRequestMessage ) ) ).Returns( request );

        return serviceProvider.Object;
    }

    private static IEdmModel NewEdm( ApiVersion apiVersion )
    {
        var builder = new ODataModelBuilder();

        builder.EntitySet<TestEntity>( "Tests" ).EntityType.HasKey( t => t.Id );

        var model = builder.GetEdmModel();

        model.SetAnnotationValue( model, new ApiVersionAnnotation( apiVersion ) );

        return model;
    }
}