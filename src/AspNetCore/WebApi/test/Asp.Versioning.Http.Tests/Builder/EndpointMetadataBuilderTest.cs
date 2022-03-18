// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;

public class EndpointMetadataBuilderTest
{
    [Fact]
    public void build_should_return_version_neutral_api_version_model()
    {
        // arrange
        var apiBuilder = Mock.Of<IVersionedApiBuilder>();
        var conventionBuilder = Mock.Of<IEndpointConventionBuilder>();
        var metadataBuilder = new EndpointMetadataBuilder( apiBuilder, conventionBuilder );

        metadataBuilder.IsApiVersionNeutral();

        // act
        var metadata = metadataBuilder.Build();

        // assert
        metadata.Should().BeSameAs( ApiVersionMetadata.Neutral );
    }

    [Fact]
    public void build_should_return_version_neutral_api_version_model_for_entire_api()
    {
        // arrange
        var apiBuilder = new Mock<IVersionedApiBuilder>();
        var conventionBuilder = Mock.Of<IEndpointConventionBuilder>();

        apiBuilder.Setup( b => b.Build() ).Returns( ApiVersionModel.Neutral );

        var metadataBuilder = new EndpointMetadataBuilder( apiBuilder.Object, conventionBuilder );

        // act
        var metadata = metadataBuilder.Build();

        // assert
        metadata.Should().BeSameAs( ApiVersionMetadata.Neutral );
    }

    [Fact]
    public void build_should_return_version_neutral_api_version_model_for_entire_api_with_name()
    {
        // arrange
        var apiBuilder = new Mock<IVersionedApiBuilder>();
        var conventionBuilder = Mock.Of<IEndpointConventionBuilder>();

        apiBuilder.Setup( b => b.Build() ).Returns( ApiVersionModel.Neutral );
        apiBuilder.SetupGet( b => b.Name ).Returns( "Test" );

        var metadataBuilder = new EndpointMetadataBuilder( apiBuilder.Object, conventionBuilder );
        var expected = new ApiVersionMetadata( ApiVersionModel.Neutral, ApiVersionModel.Neutral, "Test" );

        // act
        var metadata = metadataBuilder.Build();

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void build_should_return_empty_api_version_model()
    {
        // arrange
        var apiBuilder = new Mock<IVersionedApiBuilder>();
        var conventionBuilder = Mock.Of<IEndpointConventionBuilder>();

        apiBuilder.Setup( b => b.Build() ).Returns( ApiVersionModel.Empty );
        apiBuilder.SetupGet( b => b.Name ).Returns( "Test" );

        var metadataBuilder = new EndpointMetadataBuilder( apiBuilder.Object, conventionBuilder );
        var expected = new ApiVersionMetadata( ApiVersionModel.Empty, ApiVersionModel.Empty, "Test" );

        // act
        var metadata = metadataBuilder.Build();

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void build_should_return_inherited_api_version_model()
    {
        // arrange
        var apiBuilder = new Mock<IVersionedApiBuilder>();
        var conventionBuilder = Mock.Of<IEndpointConventionBuilder>();
        var apiModel = new ApiVersionModel( new ApiVersion( 2.0 ) );

        apiBuilder.Setup( b => b.Build() ).Returns( apiModel );
        apiBuilder.SetupGet( b => b.Name ).Returns( "Test" );

        var metadataBuilder = new EndpointMetadataBuilder( apiBuilder.Object, conventionBuilder );
        var expected = new ApiVersionMetadata( apiModel, apiModel, "Test" );

        // act
        var metadata = metadataBuilder.Build();

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void build_should_return_explicit_api_version_model()
    {
        // arrange
        var apiBuilder = new Mock<IVersionedApiBuilder>();
        var conventionBuilder = Mock.Of<IEndpointConventionBuilder>();

        apiBuilder.Setup( b => b.Build() ).Returns( ApiVersionModel.Empty );
        apiBuilder.SetupGet( b => b.Name ).Returns( "Test" );

        var metadataBuilder = new EndpointMetadataBuilder( apiBuilder.Object, conventionBuilder );
        var expected = new ApiVersionMetadata(
            ApiVersionModel.Empty,
            new ApiVersionModel(
                declaredVersions: new ApiVersion[] { new( 0.9 ), new( 1.0 ) },
                supportedVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                deprecatedVersions: new ApiVersion[] { new( 0.9 ), new( 2.0, "Beta" ) },
                advertisedVersions: new ApiVersion[] { new( 2.0 ) },
                deprecatedAdvertisedVersions: new ApiVersion[] { new( 2.0, "Beta" ) } ),
            "Test" );

        metadataBuilder.HasApiVersion( 1.0 )
                       .HasDeprecatedApiVersion( 0.9 )
                       .AdvertisesApiVersion( 2.0 )
                       .AdvertisesDeprecatedApiVersion( 2.0, "Beta" );

        // act
        var metadata = metadataBuilder.Build();

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }

    [Fact]
    public void build_should_return_mapped_api_version_model()
    {
        // arrange
        var apiBuilder = new Mock<IVersionedApiBuilder>();
        var conventionBuilder = Mock.Of<IEndpointConventionBuilder>();
        var apiModel = new ApiVersionModel(
                declaredVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                supportedVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                deprecatedVersions: Array.Empty<ApiVersion>(),
                advertisedVersions: Array.Empty<ApiVersion>(),
                deprecatedAdvertisedVersions: Array.Empty<ApiVersion>() );

        apiBuilder.Setup( b => b.Build() ).Returns( ApiVersionModel.Empty );
        apiBuilder.SetupGet( b => b.Name ).Returns( "Test" );

        var metadataBuilder = new EndpointMetadataBuilder( apiBuilder.Object, conventionBuilder );
        var expected = new ApiVersionMetadata(
            apiModel,
            new ApiVersionModel(
                declaredVersions: new ApiVersion[] { new( 2.0 ) },
                supportedVersions: new ApiVersion[] { new( 1.0 ), new( 2.0 ) },
                deprecatedVersions: Array.Empty<ApiVersion>(),
                advertisedVersions: Array.Empty<ApiVersion>(),
                deprecatedAdvertisedVersions: Array.Empty<ApiVersion>() ),
            "Test" );

        metadataBuilder.MapToApiVersion( 2.0 );

        // act
        var metadata = metadataBuilder.Build();

        // assert
        metadata.Should().BeEquivalentTo( expected );
    }
}