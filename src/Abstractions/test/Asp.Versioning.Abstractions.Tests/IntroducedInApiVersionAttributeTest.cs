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

    [Fact]
    public void introduced_in_api_version_attribute_should_compare_status_code_for_equality()
    {
        // arrange
        var version = new IntroducedInApiVersionAttribute( "2026-12-01" ) { StatusCode = 404 };
        var sameVersionAndStatus = new IntroducedInApiVersionAttribute( "2026-12-01" ) { StatusCode = 404 };
        var sameVersionDifferentStatus = new IntroducedInApiVersionAttribute( "2026-12-01" ) { StatusCode = 410 };
        var differentVersionSameStatus = new IntroducedInApiVersionAttribute( "2027-06-01" ) { StatusCode = 404 };

        // act
        var same = version.Equals( sameVersionAndStatus );
        var differentStatus = version.Equals( sameVersionDifferentStatus );
        var differentVersion = version.Equals( differentVersionSameStatus );

        // assert
        same.Should().BeTrue();
        version.GetHashCode().Should().Be( sameVersionAndStatus.GetHashCode() );
        differentStatus.Should().BeFalse();
        version.GetHashCode().Should().NotBe( sameVersionDifferentStatus.GetHashCode() );
        differentVersion.Should().BeFalse();
    }

    [Fact]
    public void introduced_in_api_version_attribute_should_not_equal_derived_type()
    {
        // arrange
        var version = new IntroducedInApiVersionAttribute( "2026-12-01" ) { StatusCode = 404 };
        var derived = new DerivedIntroducedInApiVersionAttribute( "2026-12-01" ) { StatusCode = 404 };

        // act
        var baseEqualsDerived = version.Equals( derived );
        var derivedEqualsBase = derived.Equals( version );

        // assert
        baseEqualsDerived.Should().BeFalse();
        derivedEqualsBase.Should().BeFalse();
    }

    private sealed class DerivedIntroducedInApiVersionAttribute : IntroducedInApiVersionAttribute
    {
        public DerivedIntroducedInApiVersionAttribute( string version ) : base( version ) { }
    }
}