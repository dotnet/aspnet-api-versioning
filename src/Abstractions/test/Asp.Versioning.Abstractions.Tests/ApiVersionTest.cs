// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: DX

namespace Asp.Versioning;

public partial class ApiVersionTest
{
    [Fact]
    public void new_api_version_should_not_allow_major_version_lt_0()
    {
        // arrange
        var majorVersion = -1;

        // act
        var ctor = () => new ApiVersion( majorVersion, 0 );

        // assert
        ctor.Should().Throw<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( majorVersion ) );
    }

    [Fact]
    public void new_api_version_with_status_should_not_allow_major_version_lt_0()
    {
        // arrange
        var majorVersion = -1;

        // act
        var ctor = () => new ApiVersion( majorVersion, 0, "Alpha" );

        // assert
        ctor.Should().Throw<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( majorVersion ) );
    }

    [Fact]
    public void new_api_version_with_group_version_should_not_allow_major_version_lt_0()
    {
        // arrange
        var majorVersion = -1;

        // act
        var ctor = () => new ApiVersion( Today, majorVersion, 0 );

        // assert
        ctor.Should().Throw<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( majorVersion ) );
    }

    [Fact]
    public void new_api_version_with_group_version_and_status_should_not_allow_major_version_lt_0()
    {
        // arrange
        var majorVersion = -1;

        // act
        var ctor = () => new ApiVersion( Today, majorVersion, 0, "Alpha" );

        // assert
        ctor.Should().Throw<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( majorVersion ) );
    }

    [Fact]
    public void new_api_version_should_not_allow_minor_version_lt_0()
    {
        // arrange
        var minorVersion = -1;

        // act
        var ctor = () => new ApiVersion( 0, minorVersion );

        // assert
        ctor.Should().Throw<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( minorVersion ) );
    }

    [Fact]
    public void new_api_version_with_status_should_not_allow_minor_version_lt_0()
    {
        // arrange
        var minorVersion = -1;

        // act
        var ctor = () => new ApiVersion( 0, minorVersion, "Alpha" );

        // assert
        ctor.Should().Throw<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( minorVersion ) );
    }

    [Fact]
    public void new_api_version_with_group_version_should_not_allow_minor_version_lt_0()
    {
        // arrange
        var minorVersion = -1;

        // act
        var ctor = () => new ApiVersion( Today, 0, minorVersion );

        // assert
        ctor.Should().Throw<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( minorVersion ) );
    }

    [Fact]
    public void new_api_version_with_group_version_and_status_should_not_allow_minor_version_lt_0()
    {
        // arrange
        var minorVersion = -1;

        // act
        var ctor = () => new ApiVersion( Today, 0, minorVersion, "Alpha" );

        // assert
        ctor.Should().Throw<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( minorVersion ) );
    }

    [Fact]
    public void new_api_version_with_major_and_minor_should_not_allow_invalid_status()
    {
        // arrange
        var status = "Custom-Status";

        // act
        var ctor = () => new ApiVersion( 1, 0, status );

        // assert
        ctor.Should().Throw<ArgumentException>().Subject.Single().ParamName.Should().Be( nameof( status ) );
    }

    [Fact]
    public void new_api_version_from_double_should_not_allow_invalid_status()
    {
        // arrange
        var status = "Custom-Status";

        // act
        var ctor = () => new ApiVersion( 1.0, status );

        // assert
        ctor.Should().Throw<ArgumentException>().Subject.Single().ParamName.Should().Be( nameof( status ) );
    }

    [Fact]
    public void new_api_version_with_group_version_should_not_allow_invalid_status()
    {
        // arrange
        var status = "Custom-Status";

        // act
        var ctor = () => new ApiVersion( Today, status );

        // assert
        ctor.Should().Throw<ArgumentException>().Subject.Single().ParamName.Should().Be( nameof( status ) );
    }

    [Fact]
    public void new_api_version_with_majorX2C_minorX2C_and_group_version_should_not_allow_invalid_status()
    {
        // arrange
        var status = "Custom-Status";

        // act
        var ctor = () => new ApiVersion( Today, 1, 0, status );

        // assert
        ctor.Should().Throw<ArgumentException>().Subject.Single().ParamName.Should().Be( nameof( status ) );
    }

    [Theory]
    [InlineData( 0.9, 0, 9 )]
    [InlineData( 1.0, 1, 0 )]
    [InlineData( 2d, 2, 0 )]
    [InlineData( 3.5, 3, 5 )]
    public void new_api_version_should_split_double_into_major_and_minor_versions( double version, int major, int minor )
    {
        // arrange


        // act
        var apiVersion = new ApiVersion( version );

        // assert
        apiVersion.MajorVersion.Should().Be( major );
        apiVersion.MinorVersion.Should().Be( minor );
    }

    [Theory]
    [InlineData( double.NaN )]
    [InlineData( double.PositiveInfinity )]
    [InlineData( double.NegativeInfinity )]
    public void new_api_version_should_not_allow_invalid_double( double version )
    {
        // arrange


        // act
        var ctor = () => new ApiVersion( version );

        // assert
        ctor.Should().Throw<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( version ) );
    }

    [Theory]
    [InlineData( "a" )]
    [InlineData( "B" )]
    [InlineData( "Alpha" )]
    [InlineData( "Beta" )]
    [InlineData( "RC1" )]
    [InlineData( "preview.1" )]
    public void is_valid_status_should_return_true_for_valid_status( string status )
    {
        // arrange


        // act
        var valid = ApiVersion.IsValidStatus( status );

        // assert
        valid.Should().BeTrue();
    }

    [Theory]
    [InlineData( "-a" )]
    [InlineData( "-B" )]
    [InlineData( "Alpha-1" )]
    [InlineData( "Beta-2" )]
    [InlineData( "RC-1" )]
    public void is_valid_status_should_return_false_for_invalid_status( string status )
    {
        // arrange


        // act
        var valid = ApiVersion.IsValidStatus( status );

        // assert
        valid.Should().BeFalse();
    }

    [Theory]
    [MemberData( nameof( FormatData ) )]
    public void to_string_with_format_should_return_expected_string( string format, string text, string formattedString )
    {
        // arrange
        var apiVersion = ApiVersionParser.Default.Parse( text );

        // act
        var @string = apiVersion.ToString( format );

        // assert
        @string.Should().Be( formattedString );
    }

    [Theory]
    [InlineData( "2013-08-06" )]
    [InlineData( "2013-08-06-Alpha" )]
    [InlineData( "1" )]
    [InlineData( "1.1" )]
    [InlineData( "1-Alpha" )]
    [InlineData( "1.1-Alpha" )]
    [InlineData( "2013-08-06.1" )]
    [InlineData( "2013-08-06.1.1" )]
    [InlineData( "2013-08-06.1-Alpha" )]
    [InlineData( "2013-08-06.1.1-Alpha" )]
    public void equals_should_return_true_when_api_versions_are_equal( string text )
    {
        // arrange
        var apiVersion = ApiVersionParser.Default.Parse( text );
        var other = ApiVersionParser.Default.Parse( text );

        // act
        var equal = apiVersion.Equals( other );

        // assert
        equal.Should().BeTrue();
    }

    [Theory]
    [InlineData( "2013-08-06" )]
    [InlineData( "2013-08-06-Alpha" )]
    [InlineData( "1" )]
    [InlineData( "1.1" )]
    [InlineData( "1-Alpha" )]
    [InlineData( "1.1-Alpha" )]
    [InlineData( "2013-08-06.1" )]
    [InlineData( "2013-08-06.1.1" )]
    [InlineData( "2013-08-06.1-Alpha" )]
    [InlineData( "2013-08-06.1.1-Alpha" )]
    public void equals_override_should_return_true_when_api_versions_are_equal( string text )
    {
        // arrange
        var apiVersion = ApiVersionParser.Default.Parse( text );
        object obj = ApiVersionParser.Default.Parse( text );

        // act
        var equal = apiVersion.Equals( obj );

        // assert
        equal.Should().BeTrue();
    }

    [Theory]
    [InlineData( "2013-08-06" )]
    [InlineData( "2013-08-06-Alpha" )]
    [InlineData( "1" )]
    [InlineData( "1.1" )]
    [InlineData( "1-Alpha" )]
    [InlineData( "1.1-Alpha" )]
    [InlineData( "2013-08-06.1" )]
    [InlineData( "2013-08-06.1.1" )]
    [InlineData( "2013-08-06.1-Alpha" )]
    [InlineData( "2013-08-06.1.1-Alpha" )]
    public void X3DX3D_should_return_true_when_api_versions_are_equal( string text )
    {
        // arrange
        var v1 = ApiVersionParser.Default.Parse( text );
        var v2 = ApiVersionParser.Default.Parse( text );

        // act
        var equal = v1 == v2;

        // assert
        equal.Should().BeTrue();
    }

    [Fact]
    public void equals_should_return_false_when_api_versions_are_not_equal()
    {
        // arrange
        var apiVersion = new ApiVersion( Today );
        var other = new ApiVersion( 1, 0 );

        // act
        var equal = apiVersion.Equals( other );

        // assert
        equal.Should().BeFalse();
    }

    [Fact]
    public void equals_override_should_return_false_when_api_versions_are_not_equal()
    {
        // arrange
        var apiVersion = new ApiVersion( Today );
        object obj = new ApiVersion( 1, 0 );

        // act
        var equal = apiVersion.Equals( obj );

        // assert
        equal.Should().BeFalse();
    }

    [Fact]
    public void ne_should_return_true_when_api_versions_are_not_equal()
    {
        // arrange
        var v1 = new ApiVersion( Today );
        var v2 = new ApiVersion( 1, 0 );

        // act
        var notEqual = v1 != v2;

        // assert
        notEqual.Should().BeTrue();
    }

    [Theory]
    [InlineData( "2013-08-06", "2013-08-06", 0 )]
    [InlineData( "2013-08-07", "2013-08-06", 1 )]
    [InlineData( "2013-08-05", "2013-08-06", -1 )]
    [InlineData( "2013-08-06", "2013-08-06-RC", 1 )]
    [InlineData( "2013-08-06-RC", "2013-08-06", -1 )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Alpha", 0 )]
    [InlineData( "2013-08-06-Beta", "2013-08-06-Alpha", 1 )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Beta", -1 )]
    [InlineData( "1", "1", 0 )]
    [InlineData( "1", "1.0", 0 )]
    [InlineData( "1.1", "1.1", 0 )]
    [InlineData( "2.0", "1.1", 1 )]
    [InlineData( "1.1", "2.0", -1 )]
    [InlineData( "1.1", "1.1-Beta", 1 )]
    [InlineData( "1.1-Beta", "1.1", -1 )]
    [InlineData( "1-Alpha", "1-Alpha", 0 )]
    [InlineData( "1-Alpha", "1.0-Alpha", 0 )]
    [InlineData( "1.1-Alpha", "1.1-Alpha", 0 )]
    [InlineData( "1.1-Beta", "1.1-Alpha", 1 )]
    [InlineData( "1.1-Alpha", "1.1-Beta", -1 )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.0", 0 )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1", 0 )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1-Beta", 1 )]
    [InlineData( "2013-08-06.1.1-Beta", "2013-08-06.1.1", -1 )]
    [InlineData( "2013-08-06.2", "2013-08-06.1.1", 1 )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.1", -1 )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha", 0 )]
    [InlineData( "2013-08-06.1.1-Beta", "2013-08-06.1.1-Alpha", 1 )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Beta", -1 )]
    public void api_version_comparisons_should_return_expected_result( string versionValue, string otherVersionValue, int expected )
    {
        // arrange
        var version = ApiVersionParser.Default.Parse( versionValue );
        var otherVersion = ApiVersionParser.Default.Parse( otherVersionValue );

        // act
        var result = version.CompareTo( otherVersion );

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( null, null, false )]
    [InlineData( null, "2013-08-06", true )]
    [InlineData( "2013-08-06", "2013-08-06", false )]
    [InlineData( "2013-08-07", "2013-08-06", false )]
    [InlineData( "2013-08-05", "2013-08-06", true )]
    [InlineData( "2013-08-06-Beta", "2013-08-06", true )]
    [InlineData( "2013-08-06", "2013-08-06-Beta", false )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Alpha", false )]
    [InlineData( "2013-08-06-Beta", "2013-08-06-Alpha", false )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Beta", true )]
    [InlineData( "1", "1", false )]
    [InlineData( "1", "1.0", false )]
    [InlineData( "1.1", "1.1", false )]
    [InlineData( "2.0", "1.1", false )]
    [InlineData( "1.1", "2.0", true )]
    [InlineData( "1.1-Alpha", "1.1", true )]
    [InlineData( "1.1", "1.1-Alpha", false )]
    [InlineData( "1-Alpha", "1-Alpha", false )]
    [InlineData( "1-Alpha", "1.0-Alpha", false )]
    [InlineData( "1.1-Alpha", "1.1-Alpha", false )]
    [InlineData( "1.1-Beta", "1.1-Alpha", false )]
    [InlineData( "1.1-Alpha", "1.1-Beta", true )]
    [InlineData( "2013-08-06.1", "2013-08-06.1", false )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.0", false )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1", false )]
    [InlineData( "2013-08-06.2", "2013-08-06.1.1", false )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.1", true )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1-RC", false )]
    [InlineData( "2013-08-06.1.1-RC", "2013-08-06.1.1", true )]
    [InlineData( "2013-08-06.1-Alpha", "2013-08-06.1-Alpha", false )]
    [InlineData( "2013-08-06.1-Alpha", "2013-08-06.1.0-Alpha", false )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha", false )]
    [InlineData( "2013-08-06.1.1-Beta", "2013-08-06.1.1-Alpha", false )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Beta", true )]
    public void api_version_1_lt_api_version_2_should_return_expected_result( string versionValue, string otherVersionValue, bool expected )
    {
        // arrange
        ApiVersionParser.Default.TryParse( versionValue, out var version );
        ApiVersionParser.Default.TryParse( otherVersionValue, out var otherVersion );

        // act
        var result = version < otherVersion;

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( null, null, true )]
    [InlineData( null, "2013-08-06", true )]
    [InlineData( "2013-08-06", "2013-08-06", true )]
    [InlineData( "2013-08-07", "2013-08-06", false )]
    [InlineData( "2013-08-05", "2013-08-06", true )]
    [InlineData( "2013-08-06-RC", "2013-08-06", true )]
    [InlineData( "2013-08-06", "2013-08-06-RC", false )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Alpha", true )]
    [InlineData( "2013-08-06-Beta", "2013-08-06-Alpha", false )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Beta", true )]
    [InlineData( "1", "1", true )]
    [InlineData( "1", "1.0", true )]
    [InlineData( "1.1", "1.1", true )]
    [InlineData( "2.0", "1.1", false )]
    [InlineData( "1.1", "2.0", true )]
    [InlineData( "1.1-Alpha", "1.1", true )]
    [InlineData( "1.1", "1.1-Alpha", false )]
    [InlineData( "1-Alpha", "1-Alpha", true )]
    [InlineData( "1-Alpha", "1.0-Alpha", true )]
    [InlineData( "1.1-Alpha", "1.1-Alpha", true )]
    [InlineData( "1.1-Beta", "1.1-Alpha", false )]
    [InlineData( "1.1-Alpha", "1.1-Beta", true )]
    [InlineData( "2013-08-06.1", "2013-08-06.1", true )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.0", true )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1", true )]
    [InlineData( "2013-08-06.2", "2013-08-06.1.1", false )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.1", true )]
    [InlineData( "2013-08-06.1.1-RC", "2013-08-06.1.1", true )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1-RC", false )]
    [InlineData( "2013-08-06.1-Alpha", "2013-08-06.1-Alpha", true )]
    [InlineData( "2013-08-06.1-Alpha", "2013-08-06.1.0-Alpha", true )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha", true )]
    [InlineData( "2013-08-06.1.1-Beta", "2013-08-06.1.1-Alpha", false )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Beta", true )]
    public void api_version_1_le_api_version_2_should_return_expected_result( string versionValue, string otherVersionValue, bool expected )
    {
        // arrange
        ApiVersionParser.Default.TryParse( versionValue, out var version );
        ApiVersionParser.Default.TryParse( otherVersionValue, out var otherVersion );

        // act
        var result = version <= otherVersion;

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( null, null, false )]
    [InlineData( null, "2013-08-06", false )]
    [InlineData( "2013-08-06", "2013-08-06", false )]
    [InlineData( "2013-08-07", "2013-08-06", true )]
    [InlineData( "2013-08-05", "2013-08-06", false )]
    [InlineData( "2013-08-06", "2013-08-06-Alpha", true )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06", false )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Alpha", false )]
    [InlineData( "2013-08-06-Beta", "2013-08-06-Alpha", true )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Beta", false )]
    [InlineData( "1", "1", false )]
    [InlineData( "1", "1.0", false )]
    [InlineData( "1.1", "1.1", false )]
    [InlineData( "2.0", "1.1", true )]
    [InlineData( "1.1", "2.0", false )]
    [InlineData( "1.1", "1.1-Beta", true )]
    [InlineData( "1.1-Beta", "1.1", false )]
    [InlineData( "1-Alpha", "1-Alpha", false )]
    [InlineData( "1-Alpha", "1.0-Alpha", false )]
    [InlineData( "1.1-Alpha", "1.1-Alpha", false )]
    [InlineData( "1.1-Beta", "1.1-Alpha", true )]
    [InlineData( "1.1-Alpha", "1.1-Beta", false )]
    [InlineData( "2013-08-06.1", "2013-08-06.1", false )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.0", false )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1", false )]
    [InlineData( "2013-08-06.2", "2013-08-06.1.1", true )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.1", false )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1-RC", true )]
    [InlineData( "2013-08-06.1.1-RC", "2013-08-06.1.1", false )]
    [InlineData( "2013-08-06.1-Alpha", "2013-08-06.1-Alpha", false )]
    [InlineData( "2013-08-06.1-Alpha", "2013-08-06.1.0-Alpha", false )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha", false )]
    [InlineData( "2013-08-06.1.1-Beta", "2013-08-06.1.1-Alpha", true )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Beta", false )]
    public void api_version_1_gt_api_version_2_should_return_expected_result( string versionValue, string otherVersionValue, bool expected )
    {
        // arrange
        ApiVersionParser.Default.TryParse( versionValue, out var version );
        ApiVersionParser.Default.TryParse( otherVersionValue, out var otherVersion );

        // act
        var result = version > otherVersion;

        // assert
        result.Should().Be( expected );
    }

    [Theory]
    [InlineData( null, null, true )]
    [InlineData( null, "2013-08-06", false )]
    [InlineData( "2013-08-06", "2013-08-06", true )]
    [InlineData( "2013-08-07", "2013-08-06", true )]
    [InlineData( "2013-08-05", "2013-08-06", false )]
    [InlineData( "2013-08-06", "2013-08-06-Alpha", true )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06", false )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Alpha", true )]
    [InlineData( "2013-08-06-Beta", "2013-08-06-Alpha", true )]
    [InlineData( "2013-08-06-Alpha", "2013-08-06-Beta", false )]
    [InlineData( "1", "1", true )]
    [InlineData( "1", "1.0", true )]
    [InlineData( "1.1", "1.1", true )]
    [InlineData( "2.0", "1.1", true )]
    [InlineData( "1.1", "2.0", false )]
    [InlineData( "1.1", "1.1-Beta", true )]
    [InlineData( "1.1-Beta", "1.1", false )]
    [InlineData( "1-Alpha", "1-Alpha", true )]
    [InlineData( "1-Alpha", "1.0-Alpha", true )]
    [InlineData( "1.1-Alpha", "1.1-Alpha", true )]
    [InlineData( "1.1-Beta", "1.1-Alpha", true )]
    [InlineData( "1.1-Alpha", "1.1-Beta", false )]
    [InlineData( "2013-08-06.1", "2013-08-06.1", true )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.0", true )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1", true )]
    [InlineData( "2013-08-06.2", "2013-08-06.1.1", true )]
    [InlineData( "2013-08-06.1", "2013-08-06.1.1", false )]
    [InlineData( "2013-08-06.1.1", "2013-08-06.1.1-RC", true )]
    [InlineData( "2013-08-06.1.1-RC", "2013-08-06.1.1", false )]
    [InlineData( "2013-08-06.1-Alpha", "2013-08-06.1-Alpha", true )]
    [InlineData( "2013-08-06.1-Alpha", "2013-08-06.1.0-Alpha", true )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha", true )]
    [InlineData( "2013-08-06.1.1-Beta", "2013-08-06.1.1-Alpha", true )]
    [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Beta", false )]
    public void api_version_1_ge_api_version_2_should_return_expected_result( string versionValue, string otherVersionValue, bool expected )
    {
        // arrange
        ApiVersionParser.Default.TryParse( versionValue, out var version );
        ApiVersionParser.Default.TryParse( otherVersionValue, out var otherVersion );

        // act
        var result = version >= otherVersion;

        // assert
        result.Should().Be( expected );
    }

    public static IEnumerable<object[]> FormatData =>
        new object[][]
        {
            [null, "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha"],
            ["", "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha"],
            ["F", "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha"],
            ["G", "2013-08-06", "2013-08-06"],
            ["GG", "2013-08-06-Alpha", "2013-08-06-Alpha"],
            ["G", "1.1", ""],
            ["G", "1.1-Alpha", ""],
            ["G", "2013-08-06.1.1", "2013-08-06"],
            ["GG", "2013-08-06.1.1-Alpha", "2013-08-06-Alpha"],
            ["V", "2013-08-06", ""],
            ["VVVV", "2013-08-06-Alpha", ""],
            ["VV", "1.1", "1.1"],
            ["VVVV", "1.1-Alpha", "1.1-Alpha"],
            ["VV", "2013-08-06.1.1", "1.1"],
            ["VVVV", "2013-08-06.1.1-Alpha", "1.1-Alpha"],
            ["S", "1.1-Alpha", "Alpha"],
            ["'v'VVV", "1.1", "v1.1"],
            ["'Major': %V, 'Minor': %v", "1.1", "Major: 1, Minor: 1"],
            ["MMM yyyy '('S')'", "2013-08-06-preview.1", "Aug 2013 (preview.1)"],
        };

#if NETFRAMEWORK
    private static DateTime Today => DateTime.Today;
#else
    private static DateOnly Today
    {
        get
        {
            var today = DateTime.Today;
            return new( today.Year, today.Month, today.Day );
        }
    }
#endif
}