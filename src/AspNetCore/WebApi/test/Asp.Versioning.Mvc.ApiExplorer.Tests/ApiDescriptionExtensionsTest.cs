// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dneutral

namespace Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Formatters;

public class ApiDescriptionExtensionsTest
{
    [Fact]
    public void get_api_version_should_return_associated_value()
    {
        // arrange
        var version = new ApiVersion( 42, 0 );
        var description = new ApiDescription();

        description.Properties[typeof( ApiVersion )] = version;

        // act
        var value = description.GetApiVersion();

        // assert
        value.Should().Be( version );
    }

    [Fact]
    public void set_api_version_should_associate_value()
    {
        // arrange
        var version = new ApiVersion( 42, 0 );
        var description = new ApiDescription();

        description.SetApiVersion( version );

        // act
        var value = (ApiVersion) description.Properties[typeof( ApiVersion )];

        // assert
        value.Should().Be( version );
    }

    [Theory]
    [InlineData( 0, 9, true )]
    [InlineData( 1, 0, false )]
    public void is_deprecated_should_match_model( int majorVersion, int minorVersion, bool expected )
    {
        // arrange
        var apiVersion = new ApiVersion( majorVersion, minorVersion );
        var metadata = new ApiVersionMetadata(
            ApiVersionModel.Empty,
            new ApiVersionModel(
                declaredVersions: new ApiVersion[] { new( 0, 9 ), new( 1, 0 ) },
                supportedVersions: new[] { new ApiVersion( 1, 0 ) },
                deprecatedVersions: new[] { new ApiVersion( 0, 9 ) },
                advertisedVersions: Array.Empty<ApiVersion>(),
                deprecatedAdvertisedVersions: Array.Empty<ApiVersion>() ) );
        var description = new ApiDescription()
        {
            ActionDescriptor = new() { EndpointMetadata = new[] { metadata } },
            Properties = { [typeof( ApiVersion )] = apiVersion },
        };

        // act
        var deprecated = description.IsDeprecated();

        // assert
        deprecated.Should().Be( expected );
    }

    [Fact]
    public void is_deprecated_should_return_false_for_versionX2Dneutral_action()
    {
        // arrange
        var metadata = ApiVersionMetadata.Neutral;
        var description = new ApiDescription()
        {
            ActionDescriptor = new() { EndpointMetadata = new[] { metadata } },
        };

        // act
        var deprecated = description.IsDeprecated();

        // assert
        deprecated.Should().BeFalse();
    }

    [Fact]
    public void clone_api_description_should_create_a_shallow_copy()
    {
        // arrange
        var original = new ApiDescription()
        {
            GroupName = "Test",
            HttpMethod = "GET",
            RelativePath = "test",
            ActionDescriptor = new(),
            Properties = { ["key"] = new object() },
            ParameterDescriptions = { new() },
            SupportedRequestFormats =
            {
                new()
                {
                    Formatter = Mock.Of<IInputFormatter>(),
                    MediaType = "application/json",
                },
            },
            SupportedResponseTypes =
            {
                new()
                {
                    ApiResponseFormats =
                    {
                        new()
                        {
                            Formatter = Mock.Of<IOutputFormatter>(),
                            MediaType = "application/json",
                        },
                    },
                    StatusCode = 200,
                    Type = typeof( object ),
                },
            },
        };

        // act
        var clone = original.Clone();

        // assert
        clone.Should().BeEquivalentTo( original );
    }
}