// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

#if NETFRAMEWORK
using DateOnly = System.DateTime;
#endif

public class IntroducedInApiVersionAttributeTest
{
    [Fact]
    public void introduced_in_api_version_attribute_should_initialize_from_string()
    {
        // arrange
        var expected = new ApiVersion( new DateOnly( 2026, 12, 1 ) );

        // act
        var attribute = new IntroducedInApiVersionAttribute( "2026-12-01" );

        // assert
        attribute.Versions[0].Should().Be( expected );
    }

    [Fact]
    public void introduced_in_api_version_attribute_should_initialize_from_date()
    {
        // arrange
        var expected = new ApiVersion( new DateOnly( 2026, 12, 1 ) );

        // act
        var attribute = new IntroducedInApiVersionAttribute( 2026, 12, 1 );

        // assert
        attribute.Versions[0].Should().Be( expected );
    }

    [Fact]
    public void introduced_in_api_version_attribute_should_use_default_status_code()
    {
        // arrange
        var provider = new IntroducedInApiVersionAttribute( "2026-12-01" );

        // act
        var statusCode = provider.StatusCode;

        // assert
        statusCode.Should().Be( IntroducedInApiVersionAttribute.DefaultStatusCode );
    }

    [Fact]
    public void introduced_in_api_version_attribute_should_allow_configured_status_code()
    {
        // arrange
        var provider = new IntroducedInApiVersionAttribute( "2026-12-01" )
        {
            StatusCode = IntroducedInApiVersionAttribute.UseConfiguredStatusCode,
        };

        // act
        var statusCode = provider.StatusCode;

        // assert
        statusCode.Should().Be( IntroducedInApiVersionAttribute.UseConfiguredStatusCode );
    }
}