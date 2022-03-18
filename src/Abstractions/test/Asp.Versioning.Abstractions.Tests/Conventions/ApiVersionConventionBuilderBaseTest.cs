// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

public class ApiVersionConventionBuilderBaseTest
{
    [Fact]
    public void merge_should_set_version_neutral_from_attribute()
    {
        // arrange
        var attributes = new object[]
        {
            new ApiVersionNeutralAttribute(),
        };
        var builder = new TestApiVersionConventionBuilder();

        // act
        builder.MergeAttributes( attributes );

        // assert
        builder.Build().IsApiVersionNeutral.Should().BeTrue();
    }

    [Fact]
    public void merge_should_ignore_attributes_once_version_neutral()
    {
        // arrange
        var attributes = new object[]
        {
            new ApiVersionAttribute( 1.0 ),
            new ApiVersionNeutralAttribute(),
        };
        var builder = new TestApiVersionConventionBuilder();

        // act
        builder.MergeAttributes( attributes );

        // assert
        builder.Build().Should().BeSameAs( ApiVersionModel.Neutral );
    }

    [Fact]
    public void merge_should_add_api_versions_from_attributes()
    {
        // arrange
        var attributes = new object[]
        {
            new ApiVersionAttribute( 1.0 ),
            new ApiVersionAttribute( 0.9 ) { Deprecated = true },
            new AdvertiseApiVersionsAttribute( 2.0 ),
            new AdvertiseApiVersionsAttribute( "2.0-Beta" ) { Deprecated = true },
        };
        var expected = new ApiVersionModel(
            new ApiVersion[] { new( 1.0 ) },
            new ApiVersion[] { new( 0.9 ) },
            new ApiVersion[] { new( 2.0 ) },
            new ApiVersion[] { new( 2.0, "Beta" ) } );
        var builder = new TestApiVersionConventionBuilder();

        // act
        builder.MergeAttributes( attributes );

        // assert
        builder.Build().Should().BeEquivalentTo( expected );
    }

    private sealed class TestApiVersionConventionBuilder : ApiVersionConventionBuilderBase
    {
        public void MergeAttributes( IReadOnlyList<object> attributes ) =>
            MergeAttributesWithConventions( attributes );

        public ApiVersionModel Build()
        {
            if ( VersionNeutral )
            {
                return ApiVersionModel.Neutral;
            }

            return new(
                SupportedVersions,
                DeprecatedVersions,
                AdvertisedVersions,
                DeprecatedAdvertisedVersions );
        }
    }
}