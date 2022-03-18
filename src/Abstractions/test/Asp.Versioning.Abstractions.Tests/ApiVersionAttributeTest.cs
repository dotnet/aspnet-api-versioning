// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class ApiVersionAttributeTest
{
    [Theory]
    [InlineData( 0, 9, "0.9" )]
    [InlineData( 2, 1, "2.1" )]
    [InlineData( 3, 0, "3.0" )]
    public void api_version_attribute_should_initialize_from_string( int major, int minor, string value )
    {
        // arrange
        var expected = new ApiVersion( major, minor );
        var attribute = new ApiVersionAttribute( value );

        // act
        var versions = attribute.Versions;

        // assert
        versions[0].Should().Be( expected );
    }

    [Theory]
    [InlineData( 0, 9, 0.9 )]
    [InlineData( 2, 1, 2.1 )]
    [InlineData( 3, 0, 3.0 )]
    public void api_version_attribute_should_initialize_from_double( int major, int minor, double value )
    {
        // arrange
        var expected = new ApiVersion( major, minor );
        var attribute = new ApiVersionAttribute( value );

        // act
        var versions = attribute.Versions;

        // assert
        versions[0].Should().Be( expected );
    }

    [Fact]
    public void api_version_attribute_should_initialize_from_double_with_status()
    {
        // arrange
        var expected = new ApiVersion( 2.0, "alpha" );

        // act
        var attribute = new ApiVersionAttribute( 2.0, "alpha" );

        // assert
        attribute.Versions[0].Should().Be( expected );
    }
}