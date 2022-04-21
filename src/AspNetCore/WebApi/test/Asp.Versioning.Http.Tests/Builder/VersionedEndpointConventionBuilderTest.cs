// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;

public class VersionedEndpointConventionBuilderTest
{
    [Fact]
    public void build_should_return_version_neutral_api_version_model()
    {
        // arrange
        var endpoints = Mock.Of<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).Build();
        var builder = endpoints.UseApiVersioning( versionSet );

        builder.IsApiVersionNeutral();

        // act
        var metadata = builder.Build( new() );

        // assert
        metadata.Should().BeSameAs( ApiVersionMetadata.Neutral );
    }

    [Fact]
    public void build_should_return_version_neutral_api_version_model_for_entire_api()
    {
        // arrange
        var endpoints = Mock.Of<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( default ).IsApiVersionNeutral().Build();
        var builder = endpoints.UseApiVersioning( versionSet );

        // act
        var metadata = builder.Build( new() );

        // assert
        metadata.Should().BeSameAs( ApiVersionMetadata.Neutral );
    }

    [Fact]
    public void build_should_return_version_neutral_api_version_model_for_entire_api_with_name()
    {
        // arrange
        var endpoints = Mock.Of<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( "Test" ).IsApiVersionNeutral().Build();
        var builder = endpoints.UseApiVersioning( versionSet );
        var expected = new ApiVersionMetadata( ApiVersionModel.Neutral, ApiVersionModel.Neutral, "Test" );

        // act
        var metadata = builder.Build( new() );

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void build_should_return_empty_api_version_model()
    {
        // arrange
        var endpoints = Mock.Of<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( "Test" ).Build();
        var builder = endpoints.UseApiVersioning( versionSet );
        var expected = new ApiVersionMetadata( ApiVersionModel.Empty, ApiVersionModel.Empty, "Test" );

        // act
        var metadata = builder.Build( new() );

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void build_should_return_inherited_api_version_model()
    {
        // arrange
        var endpoints = Mock.Of<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( "Test" ).HasApiVersion( 2.0 ).Build();
        var builder = endpoints.UseApiVersioning( versionSet );
        var apiModel = new ApiVersionModel( new ApiVersion( 2.0 ) );
        var expected = new ApiVersionMetadata( apiModel, apiModel, "Test" );

        // act
        var metadata = builder.Build( new() );

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void build_should_return_explicit_api_version_model()
    {
        // arrange
        var endpoints = Mock.Of<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( "Test" ).Build();
        var builder = endpoints.UseApiVersioning( versionSet );
        var expected = new ApiVersionMetadata(
            ApiVersionModel.Empty,
            new ApiVersionModel(
                declaredVersions: new ApiVersion[] { new( 0.9 ), new( 1.0 ) },
                supportedVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                deprecatedVersions: new ApiVersion[] { new( 0.9 ), new( 2.0, "Beta" ) },
                advertisedVersions: new ApiVersion[] { new( 2.0 ) },
                deprecatedAdvertisedVersions: new ApiVersion[] { new( 2.0, "Beta" ) } ),
            "Test" );

        builder.HasApiVersion( 1.0 )
               .HasDeprecatedApiVersion( 0.9 )
               .AdvertisesApiVersion( 2.0 )
               .AdvertisesDeprecatedApiVersion( 2.0, "Beta" );

        // act
        var metadata = builder.Build( new() );

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void build_should_return_mapped_api_version_model()
    {
        // arrange
        var endpoints = Mock.Of<IEndpointConventionBuilder>();
        var versionSet = new ApiVersionSetBuilder( "Test" )
            .HasApiVersion( 1.0 )
            .HasApiVersion( 2.0 )
            .Build();
        var builder = endpoints.UseApiVersioning( versionSet );
        var apiModel = new ApiVersionModel(
                declaredVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                supportedVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                deprecatedVersions: Array.Empty<ApiVersion>(),
                advertisedVersions: Array.Empty<ApiVersion>(),
                deprecatedAdvertisedVersions: Array.Empty<ApiVersion>() );
        var expected = new ApiVersionMetadata(
            apiModel,
            new ApiVersionModel(
                declaredVersions: new ApiVersion[] { new( 2.0 ) },
                supportedVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                deprecatedVersions: Array.Empty<ApiVersion>(),
                advertisedVersions: Array.Empty<ApiVersion>(),
                deprecatedAdvertisedVersions: Array.Empty<ApiVersion>() ),
            "Test" );

        builder.MapToApiVersion( 2.0 );

        // act
        var metadata = builder.Build( new() );

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }
}