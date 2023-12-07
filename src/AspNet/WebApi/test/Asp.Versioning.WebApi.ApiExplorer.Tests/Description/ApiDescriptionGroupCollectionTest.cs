// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: denormalized

namespace Asp.Versioning.Description;

using System.Collections.ObjectModel;

public class ApiDescriptionGroupCollectionTest
{
    [Fact]
    public void versions_should_return_sorted_values()
    {
        // arrange
        var collection = new ApiDescriptionGroupCollection()
        {
            new( new ApiVersion( 3, 0 ), "V3" ),
            new( new ApiVersion( 1, 0 ), "V1" ),
            new( new ApiVersion( 2, 0 ), "V2" ),
        };

        // act
        var versions = collection.ApiVersions;

        // assert
        versions.Should().BeEquivalentTo( new ApiVersion[] { new( 1, 0 ), new( 2, 0 ), new( 3, 0 ) } );
    }

    [Fact]
    public void flatten_should_return_denormalized_api_descriptions_in_order()
    {
        // arrange
        var collection = new ApiDescriptionGroupCollection()
            {
                new( new ApiVersion( 3, 0 ), "V3" )
                {
                    ApiDescriptions =
                    {
                        new() { ApiVersion = new( 3, 0 ), RelativePath = "api/values" },
                        new() { ApiVersion = new( 3, 0 ), RelativePath = "api/orders" },
                    },
                },
                new( new ApiVersion( 1, 0 ), "V1" )
                {
                    ApiDescriptions =
                    {
                        new() { ApiVersion = new( 1, 0 ), RelativePath = "api/people" },
                        new() { ApiVersion = new( 1, 0 ), RelativePath = "api/orders" },
                    },
                },
                new( new ApiVersion( 2, 0 ), "V2" )
                {
                    ApiDescriptions =
                    {
                        new() { ApiVersion = new( 2, 0 ), RelativePath = "api/values" },
                        new() { ApiVersion = new( 2, 0 ), RelativePath = "api/people" },
                        new() { ApiVersion = new( 2, 0 ), RelativePath = "api/orders" },
                    },
                },
            };

        // act
        var descriptions = collection.Flatten().Cast<VersionedApiDescription>();

        // assert
        descriptions.Should().BeEquivalentTo(
            new Collection<VersionedApiDescription>()
            {
                    new() { ApiVersion = new( 1, 0 ), RelativePath = "api/people" },
                    new() { ApiVersion = new( 1, 0 ), RelativePath = "api/orders" },
                    new() { ApiVersion = new( 2, 0 ), RelativePath = "api/values" },
                    new() { ApiVersion = new( 2, 0 ), RelativePath = "api/people" },
                    new() { ApiVersion = new( 2, 0 ), RelativePath = "api/orders" },
                    new() { ApiVersion = new( 3, 0 ), RelativePath = "api/values" },
                    new() { ApiVersion = new( 3, 0 ), RelativePath = "api/orders" },
            } );
    }
}