// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using static Asp.Versioning.ApiVersionProviderOptions;
#if NETFRAMEWORK
using DateOnly = System.DateTime;
#endif

public class AdvertiseApiVersionsAttributeTest
{
    [Theory]
    [InlineData( false, Advertised )]
    [InlineData( true, Advertised | Deprecated )]
    public void new_advertise_api_versions_attribute_should_have_expected_options( bool deprecated, ApiVersionProviderOptions expected )
    {
        // arrange
        IApiVersionProvider attribute = new AdvertiseApiVersionsAttribute( 42.0 ) { Deprecated = deprecated };

        // act
        var options = attribute.Options;

        // assert
        options.Should().Be( expected );
    }

    [Fact]
    public void advertise_api_versions_base_attribute_should_initialize_from_array_of_double()
    {
        // arrange
        var version = 1.0;
        var otherVersions = new[] { 2.0, 3.0 };

        // act
        var attribute = new AdvertiseApiVersionsAttribute( version, otherVersions );

        // asserts
        attribute.Versions.Should().BeEquivalentTo( new ApiVersion[] { new( 1.0 ), new( 2.0 ), new( 3.0 ) } );
    }

    [Fact]
    public void advertise_api_versions_base_attribute_should_initialize_from_array_of_string()
    {
        // arrange
        var version = "1.0";
        var otherVersions = new[] { "2.0", "3.0" };

        // act
        var attribute = new AdvertiseApiVersionsAttribute( version, otherVersions );

        // assert
        attribute.Versions.Should().BeEquivalentTo( new ApiVersion[] { new( 1.0 ), new( 2.0 ), new( 3.0 ) } );
    }

    [Fact]
    public void advertise_api_version_attribute_should_initialize_from_date()
    {
        // arrange
        var expected = new ApiVersion( new DateOnly( 2016, 1, 1 ) );

        // act
        var attribute = new AdvertiseApiVersionsAttribute( 2016, 1, 1 );

        // assert
        attribute.Versions[0].Should().Be( expected );
    }

    [Fact]
    public void advertise_api_version_attribute_should_initialize_from_date_with_status()
    {
        // arrange
        var expected = new ApiVersion( new DateOnly( 2016, 1, 1 ), "alpha" );

        // act
        var attribute = new AdvertiseApiVersionsAttribute( 2016, 1, 1, "alpha" );

        // assert
        attribute.Versions[0].Should().Be( expected );
    }
}