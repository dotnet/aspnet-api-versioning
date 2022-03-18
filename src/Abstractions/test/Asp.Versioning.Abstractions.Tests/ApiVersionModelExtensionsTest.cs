// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class ApiVersionModelExtensionsTest
{
    [Fact]
    public void aggregate_should_merge_api_version_models()
    {
        // arrange
        var model1 = new ApiVersionModel( new[] { new ApiVersion( 1, 0 ) }, new[] { new ApiVersion( 0, 9 ) } );
        var model2 = new ApiVersionModel( new[] { new ApiVersion( 2, 0 ) }, Enumerable.Empty<ApiVersion>() );
        var expected = new ApiVersionModel( new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) }, new[] { new ApiVersion( 0, 9 ) } );

        // act
        var aggregatedModel = model1.Aggregate( model2 );

        // assert
        aggregatedModel.Should().BeEquivalentTo(
            new
            {
                expected.IsApiVersionNeutral,
                expected.DeclaredApiVersions,
                expected.ImplementedApiVersions,
                expected.SupportedApiVersions,
                expected.DeprecatedApiVersions,
            } );
    }

    [Fact]
    public void aggregate_should_merge_api_version_model_sequence()
    {
        // arrange
        var model = new ApiVersionModel( new[] { new ApiVersion( 1, 0 ) }, new[] { new ApiVersion( 0, 9 ) } );
        var otherModels = new[]
        {
                new ApiVersionModel( new[] { new ApiVersion( 2, 0 ) }, Enumerable.Empty<ApiVersion>() ),
                new ApiVersionModel( new[] { new ApiVersion( 3, 0 ) }, new[] { new ApiVersion( 3, 0, "Alpha" ) } ),
        };
        var expected = new ApiVersionModel(
            new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ), new ApiVersion( 3, 0 ) },
            new[] { new ApiVersion( 0, 9 ), new ApiVersion( 3, 0, "Alpha" ) } );

        // act
        var aggregatedModel = model.Aggregate( otherModels );

        // assert
        aggregatedModel.Should().BeEquivalentTo(
            new
            {
                expected.IsApiVersionNeutral,
                expected.DeclaredApiVersions,
                expected.ImplementedApiVersions,
                expected.SupportedApiVersions,
                expected.DeprecatedApiVersions,
            } );
    }

    [Fact]
    public void aggregate_should_not_merge_deprecated_api_version_when_also_supported()
    {
        // arrange
        var model1 = new ApiVersionModel( new[] { new ApiVersion( 1, 0 ) }, Enumerable.Empty<ApiVersion>() );
        var model2 = new ApiVersionModel( new[] { new ApiVersion( 2, 0 ) }, new[] { new ApiVersion( 1, 0 ) } );
        var expected = new ApiVersionModel( new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) }, Enumerable.Empty<ApiVersion>() );

        // act
        var aggregatedModel = model1.Aggregate( model2 );

        // assert
        aggregatedModel.Should().BeEquivalentTo(
            new
            {
                expected.IsApiVersionNeutral,
                expected.DeclaredApiVersions,
                expected.ImplementedApiVersions,
                expected.SupportedApiVersions,
                expected.DeprecatedApiVersions,
            } );
    }
}