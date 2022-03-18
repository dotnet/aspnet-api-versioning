// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static Asp.Versioning.ApiVersionMapping;

public class ApiVersionMetadataTest
{
    [Fact]
    public void map_should_return_explicit_model()
    {
        // arrange
        var apiVersion = new ApiVersion( 42, 0 );
        var apiModel = ApiVersionModel.Empty;
        var endpointModel = new ApiVersionModel( apiVersion );
        var metadata = new ApiVersionMetadata( apiModel, endpointModel );

        // act
        var result = metadata.Map( Explicit );

        // assert
        result.Should().BeSameAs( endpointModel );
    }

    [Fact]
    public void map_should_return_implicit_model()
    {
        // arrange
        var apiVersion = new ApiVersion( 42, 0 );
        var apiModel = new ApiVersionModel( apiVersion );
        var endpointModel = ApiVersionModel.Empty;
        var metadata = new ApiVersionMetadata( apiModel, endpointModel );

        // act
        var result = metadata.Map( Implicit );

        // assert
        result.Should().BeSameAs( apiModel );
    }

    [Fact]
    public void map_should_return_merge_models()
    {
        // arrange
        var apiModel = new ApiVersionModel( new ApiVersion( 1.0 ) );
        var endpointModel = new ApiVersionModel( new ApiVersion[] { new( 1.0 ) }, new ApiVersion[] { new( 0.9 ) } );
        var metadata = new ApiVersionMetadata( apiModel, endpointModel );
        var expected = new ApiVersionModel(
            new ApiVersion[] { new( 1.0 ) },
            new ApiVersion[] { new( 1.0 ) },
            new ApiVersion[] { new( 0.9 ) },
            Enumerable.Empty<ApiVersion>(),
            Enumerable.Empty<ApiVersion>() );

        // act
        var result = metadata.Map( Explicit | Implicit );

        // assert
        result.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void mapping_to_should_be_implicit_when_no_api_versions_have_been_mapped()
    {
        // arrange
        var apiVersion = new ApiVersion( 1, 0 );
        var apiModel = new ApiVersionModel( apiVersion );
        var endpointModel = ApiVersionModel.Empty;
        var metadata = new ApiVersionMetadata( apiModel, endpointModel );

        // act
        var result = metadata.MappingTo( apiVersion );

        // assert
        result.Should().Be( Implicit );
    }

    [Fact]
    public void mapping_to_should_not_implicitly_map_to_specific_api_version()
    {
        // arrange
        var apiVersion = new ApiVersion( 42, 0 );
        var metadata = ApiVersionMetadata.Empty;

        // act
        var result = metadata.MappingTo( apiVersion );

        // assert
        result.Should().Be( None );
    }

    [Fact]
    public void mapping_to_should_explicitly_map_to_specific_api_version()
    {
        // arrange
        var apiVersion = new ApiVersion( 42, 0 );
        var apiModel = ApiVersionModel.Empty;
        var endpointModel = new ApiVersionModel( apiVersion );
        var metadata = new ApiVersionMetadata( apiModel, endpointModel );

        // act
        var result = metadata.MappingTo( apiVersion );

        // assert
        result.Should().Be( Explicit );
    }

    [Fact]
    public void is_mapped_to_should_be_true_when_api_version_is_explicitly_mapped()
    {
        // arrange
        var apiVersion = new ApiVersion( 42, 0 );
        var apiModel = ApiVersionModel.Empty;
        var endpointModel = new ApiVersionModel( apiVersion );
        var metadata = new ApiVersionMetadata( apiModel, endpointModel );

        // act
        var result = metadata.IsMappedTo( apiVersion );

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void is_mapped_to_should_be_true_when_api_version_is_implicitly_mapped()
    {
        // arrange
        var apiVersion = new ApiVersion( 42, 0 );
        var apiModel = new ApiVersionModel( apiVersion );
        var endpointModel = ApiVersionModel.Empty;
        var metadata = new ApiVersionMetadata( apiModel, endpointModel );

        // act
        var result = metadata.IsMappedTo( apiVersion );

        // assert
        result.Should().BeTrue();
    }

    [Fact]
    public void is_mapped_to_should_be_false_when_api_version_is_not_mapped()
    {
        // arrange
        var apiVersion = new ApiVersion( 42, 0 );
        var metadata = ApiVersionMetadata.Empty;

        // act
        var result = metadata.IsMappedTo( apiVersion );

        // assert
        result.Should().BeFalse();
    }

    [Fact]
    public void deconstruct_should_decompose_metadata()
    {
        // arrange
        var apiModel = new ApiVersionModel( new ApiVersion( 1.0 ) );
        var endpointModel = new ApiVersionModel( new ApiVersion( 2.0 ) );
        var metadata = new ApiVersionMetadata( apiModel, endpointModel );

        // act
        metadata.Deconstruct( out var apiModelResult, out var endpointModelResult );

        // assert
        apiModelResult.Should().BeSameAs( apiModel );
        endpointModelResult.Should().BeSameAs( endpointModel );
    }

    [Fact]
    public void deconstruct_should_decompose_metadata_with_name()
    {
        // arrange
        var apiModel = new ApiVersionModel( new ApiVersion( 1.0 ) );
        var endpointModel = new ApiVersionModel( new ApiVersion( 2.0 ) );
        var metadata = new ApiVersionMetadata( apiModel, endpointModel, "Test" );

        // act
        metadata.Deconstruct( out var apiModelResult, out var endpointModelResult, out var name );

        // assert
        apiModelResult.Should().BeSameAs( apiModel );
        endpointModelResult.Should().BeSameAs( endpointModel );
        name.Should().Be( "Test" );
    }
}