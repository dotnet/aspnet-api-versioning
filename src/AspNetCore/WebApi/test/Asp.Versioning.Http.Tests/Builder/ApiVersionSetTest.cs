// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Builder;

public class ApiVersionSetTest
{
    [Fact]
    public void report_api_versions_should_derive_from_builder()
    {
        // arrange
        var builder = new ApiVersionSetBuilder( default ).ReportApiVersions();

        // act
        var versionSet = builder.Build();

        // assert
        versionSet.ReportApiVersions.Should().BeTrue();
    }

    [Fact]
    public void build_should_construct_model_from_builder()
    {
        // arrange
        var versionSet = new ApiVersionSetBuilder( null ).IsApiVersionNeutral().Build();

        // act
        var model = versionSet.Build( new() );

        // assert
        model.Should().BeSameAs( ApiVersionModel.Neutral );
    }

    [Fact]
    public void advertises_api_version_should_propagate_to_builder()
    {
        // arrange
        var builder = new Mock<ApiVersionSetBuilder>( null ) { CallBase = true };

        builder.Setup( b => b.AdvertisesApiVersion( It.IsAny<ApiVersion>() ) );

        var versionSet = builder.Object.Build();
        var expected = new ApiVersion( 2.0 );

        // act
        versionSet.AdvertisesApiVersion( expected );

        // assert
        builder.Verify( b => b.AdvertisesApiVersion( expected ) );
    }

    [Fact]
    public void advertises_deprecated_api_version_should_propagate_to_builder()
    {
        // arrange
        var builder = new Mock<ApiVersionSetBuilder>( null ) { CallBase = true };

        builder.Setup( b => b.AdvertisesDeprecatedApiVersion( It.IsAny<ApiVersion>() ) );

        var versionSet = builder.Object.Build();
        var expected = new ApiVersion( 0.9 );

        // act
        versionSet.AdvertisesDeprecatedApiVersion( expected );

        // assert
        builder.Verify( b => b.AdvertisesDeprecatedApiVersion( expected ) );
    }
}