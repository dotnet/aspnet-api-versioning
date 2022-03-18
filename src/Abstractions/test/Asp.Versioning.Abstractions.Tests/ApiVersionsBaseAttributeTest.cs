// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class ApiVersionsBaseAttributeTest
{
    [Fact]
    public void api_versions_base_attribute_should_initialize_from_array()
    {
        // arrange
        var version = new ApiVersion( 1.0 );
        var otherVersions = new ApiVersion[] { new( 2.0 ), new( 3.0 ) };

        // act
        var attribute = new Mock<ApiVersionsBaseAttribute>( version, otherVersions ) { CallBase = true }.Object;

        // assert
        attribute.Versions.Should().BeEquivalentTo( new ApiVersion[] { new( 1.0 ), new( 2.0 ), new( 3.0 ) } );
    }

    [Fact]
    public void api_versions_base_attribute_should_initialize_from_array_of_double()
    {
        // arrange
        var version = 1.0;
        var otherVersions = new[] { 2.0, 3.0 };

        // act
        var attribute = new Mock<ApiVersionsBaseAttribute>( version, otherVersions ) { CallBase = true }.Object;

        // asserts
        attribute.Versions.Should().BeEquivalentTo( new ApiVersion[] { new( 1.0 ), new( 2.0 ), new( 3.0 ) } );
    }

    [Fact]
    public void api_versions_base_attribute_should_initialize_from_array_of_string()
    {
        // arrange
        var version = "1.0";
        var otherVersions = new[] { "2.0", "3.0" };

        // act
        var attribute = new Mock<ApiVersionsBaseAttribute>( version, otherVersions ) { CallBase = true }.Object;

        // assert
        attribute.Versions.Should().BeEquivalentTo( new ApiVersion[] { new( 1.0 ), new( 2.0 ), new( 3.0 ) } );
    }
}