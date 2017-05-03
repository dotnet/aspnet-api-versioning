namespace Microsoft.Web.Http
{
    using FluentAssertions;
    using System;
    using System.Linq;
    using Xunit;
    using static System.DateTime;
    using static System.Globalization.CultureInfo;

    public class ApiVersionTest
    {
        [Fact]
        public void new_api_version_should_not_allow_major_version_lt_0()
        {
            // arrange
            var majorVersion = -1;

            // act
            Action ctor = () => new ApiVersion( majorVersion, 0 );

            // assert
            ctor.ShouldThrow<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( majorVersion ) );
        }

        [Fact]
        public void new_api_version_with_status_should_not_allow_major_version_lt_0()
        {
            // arrange
            var majorVersion = -1;

            // act
            Action ctor = () => new ApiVersion( majorVersion, 0, "Alpha" );

            // assert
            ctor.ShouldThrow<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( majorVersion ) );
        }

        [Fact]
        public void new_api_version_with_group_version_should_not_allow_major_version_lt_0()
        {
            // arrange
            var majorVersion = -1;

            // act
            Action ctor = () => new ApiVersion( Today, majorVersion, 0 );

            // assert
            ctor.ShouldThrow<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( majorVersion ) );
        }

        [Fact]
        public void new_api_version_with_group_version_and_status_should_not_allow_major_version_lt_0()
        {
            // arrange
            var majorVersion = -1;

            // act
            Action ctor = () => new ApiVersion( Today, majorVersion, 0, "Alpha" );

            // assert
            ctor.ShouldThrow<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( majorVersion ) );
        }

        [Fact]
        public void new_api_version_should_not_allow_minor_version_lt_0()
        {
            // arrange
            var minorVersion = -1;

            // act
            Action ctor = () => new ApiVersion( 0, minorVersion );

            // assert
            ctor.ShouldThrow<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( minorVersion ) );
        }

        [Fact]
        public void new_api_version_with_status_should_not_allow_minor_version_lt_0()
        {
            // arrange
            var minorVersion = -1;

            // act
            Action ctor = () => new ApiVersion( 0, minorVersion, "Alpha" );

            // assert
            ctor.ShouldThrow<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( minorVersion ) );
        }

        [Fact]
        public void new_api_version_with_group_version_should_not_allow_minor_version_lt_0()
        {
            // arrange
            var minorVersion = -1;

            // act
            Action ctor = () => new ApiVersion( Today, 0, minorVersion );

            // assert
            ctor.ShouldThrow<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( minorVersion ) );
        }

        [Fact]
        public void new_api_version_with_group_version_and_status_should_not_allow_minor_version_lt_0()
        {
            // arrange
            var minorVersion = -1;

            // act
            Action ctor = () => new ApiVersion( Today, 0, minorVersion, "Alpha" );

            // assert
            ctor.ShouldThrow<ArgumentOutOfRangeException>().Subject.Single().ParamName.Should().Be( nameof( minorVersion ) );
        }

        [Fact]
        public void new_api_version_with_major_and_minor_should_not_allow_invalid_status()
        {
            // arrange
            var status = "Custom-Status";

            // act
            Action ctor = () => new ApiVersion( 1, 0, status );

            // assert
            ctor.ShouldThrow<ArgumentException>().Subject.Single().ParamName.Should().Be( nameof( status ) );
        }

        [Fact]
        public void new_api_version_with_group_version_should_not_allow_invalid_status()
        {
            // arrange
            var status = "Custom-Status";

            // act
            Action ctor = () => new ApiVersion( Today, status );

            // assert
            ctor.ShouldThrow<ArgumentException>().Subject.Single().ParamName.Should().Be( nameof( status ) );
        }

        [Fact]
        public void new_api_version_with_majorX2C_minorX2C_and_group_version_should_not_allow_invalid_status()
        {
            // arrange
            var status = "Custom-Status";

            // act
            Action ctor = () => new ApiVersion( Today, 1, 0, status );

            // assert
            ctor.ShouldThrow<ArgumentException>().Subject.Single().ParamName.Should().Be( nameof( status ) );
        }

        [Theory]
        [InlineData( "a" )]
        [InlineData( "B" )]
        [InlineData( "Alpha" )]
        [InlineData( "Beta" )]
        [InlineData( "RC1" )]
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
        [InlineData( "2013-08-06", "2013-08-06", null, null, null )]
        [InlineData( "2013-08-06-Alpha", "2013-08-06", null, null, "Alpha" )]
        [InlineData( "1", null, 1, null, null )]
        [InlineData( "1.1", null, 1, 1, null )]
        [InlineData( "1-Alpha", null, 1, null, "Alpha" )]
        [InlineData( "1.1-Alpha", null, 1, 1, "Alpha" )]
        [InlineData( "2013-08-06.1", "2013-08-06", 1, null, null )]
        [InlineData( "2013-08-06.1.1", "2013-08-06", 1, 1, null )]
        [InlineData( "2013-08-06.1-Alpha", "2013-08-06", 1, null, "Alpha" )]
        [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06", 1, 1, "Alpha" )]
        public void parse_should_return_expected_result( string text, string groupVersionValue, int? majorVersion, int? minorVersion, string status )
        {
            // arrange
            var groupVersion = groupVersionValue == null ? null : new DateTime?( Parse( groupVersionValue ) );

            // act
            var apiVersion = ApiVersion.Parse( text );

            // assert
            apiVersion.ShouldBeEquivalentTo(
                new
                {
                    GroupVersion = groupVersion,
                    MajorVersion = majorVersion,
                    MinorVersion = minorVersion,
                    Status = status
                } );
        }

        [Theory]
        [InlineData( "Alpha1", "The specified API version is invalid." )]
        [InlineData( "1.1-Alpha-1", "The specified API version status 'Alpha-1' is invalid." )]
        [InlineData( "2013-02-29.1.0", "The specified API group version '2013-02-29' is invalid." )]
        public void parse_should_throw_format_exception_for_invalid_text( string text, string message )
        {
            // arrange
            Action parse = () => ApiVersion.Parse( text );

            // act


            // assert
            parse.ShouldThrow<FormatException>().WithMessage( message );
        }

        [Theory]
        [InlineData( "2013-08-06", "2013-08-06", null, null, null )]
        [InlineData( "2013-08-06-Alpha", "2013-08-06", null, null, "Alpha" )]
        [InlineData( "1", null, 1, null, null )]
        [InlineData( "1.1", null, 1, 1, null )]
        [InlineData( "1-Alpha", null, 1, null, "Alpha" )]
        [InlineData( "1.1-Alpha", null, 1, 1, "Alpha" )]
        [InlineData( "2013-08-06.1", "2013-08-06", 1, null, null )]
        [InlineData( "2013-08-06.1.1", "2013-08-06", 1, 1, null )]
        [InlineData( "2013-08-06.1-Alpha", "2013-08-06", 1, null, "Alpha" )]
        [InlineData( "2013-08-06.1.1-Alpha", "2013-08-06", 1, 1, "Alpha" )]
        public void try_parse_should_return_expected_api_version( string text, string groupVersionValue, int? majorVersion, int? minorVersion, string status )
        {
            // arrange
            var groupVersion = groupVersionValue == null ? null : new DateTime?( Parse( groupVersionValue ) );
            var apiVersion = default( ApiVersion );

            // act
            var result = ApiVersion.TryParse( text, out apiVersion );

            // assert
            result.Should().BeTrue();
            apiVersion.ShouldBeEquivalentTo(
                new
                {
                    GroupVersion = groupVersion,
                    MajorVersion = majorVersion,
                    MinorVersion = minorVersion,
                    Status = status
                } );
        }

        [Theory]
        [InlineData( "Alpha1" )]
        [InlineData( "1.1-Alpha-1" )]
        [InlineData( "2013-02-29.1.0" )]
        public void try_parse_should_return_false_when_text_is_invalid( string text )
        {
            // arrange
            var apiVersion = default( ApiVersion );

            // act
            var result = ApiVersion.TryParse( text, out apiVersion );

            // assert
            result.Should().BeFalse();
            apiVersion.Should().BeNull();
        }

        [Theory]
        [InlineData( "2013-08-06" )]
        [InlineData( "2013-08-06-Alpha" )]
        [InlineData( "1" )]
        [InlineData( "1.1" )]
        [InlineData( "1.1-Alpha" )]
        [InlineData( "2013-08-06.1" )]
        [InlineData( "2013-08-06.1.1" )]
        [InlineData( "2013-08-06.1-Alpha" )]
        [InlineData( "2013-08-06.1.1-Alpha" )]
        public void to_string_should_return_expected_string( string text )
        {
            // arrange
            var apiVersion = ApiVersion.Parse( text );

            // act
            var @string = apiVersion.ToString();

            // assert
            @string.Should().Be( text );
        }

        [Theory]
        [InlineData( null, "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha" )]
        [InlineData( "", "2013-08-06.1.1-Alpha", "2013-08-06.1.1-Alpha" )]
        [InlineData( "G", "2013-08-06", "2013-08-06" )]
        [InlineData( "GG", "2013-08-06-Alpha", "2013-08-06-Alpha" )]
        [InlineData( "G", "1.1", "" )]
        [InlineData( "G", "1.1-Alpha", "" )]
        [InlineData( "G", "2013-08-06.1.1", "2013-08-06" )]
        [InlineData( "GG", "2013-08-06.1.1-Alpha", "2013-08-06-Alpha" )]
        [InlineData( "V", "2013-08-06", "" )]
        [InlineData( "VVVV", "2013-08-06-Alpha", "" )]
        [InlineData( "VV", "1.1", "1.1" )]
        [InlineData( "VVVV", "1.1-Alpha", "1.1-Alpha" )]
        [InlineData( "VV", "2013-08-06.1.1", "1.1" )]
        [InlineData( "VVVV", "2013-08-06.1.1-Alpha", "1.1-Alpha" )]
        public void to_string_with_format_should_return_expected_string( string format, string text, string formattedString )
        {
            // arrange
            var apiVersion = ApiVersion.Parse( text );

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
            var apiVersion = ApiVersion.Parse( text );
            var other = ApiVersion.Parse( text );

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
            var apiVersion = ApiVersion.Parse( text );
            object obj = ApiVersion.Parse( text );

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
            var v1 = ApiVersion.Parse( text );
            var v2 = ApiVersion.Parse( text );

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
            var version = ApiVersion.Parse( versionValue );
            var otherVersion = ApiVersion.Parse( otherVersionValue );

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
            ApiVersion version = null;
            ApiVersion otherVersion = null;
            ApiVersion.TryParse( versionValue, out version );
            ApiVersion.TryParse( otherVersionValue, out otherVersion );

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
            ApiVersion version = null;
            ApiVersion otherVersion = null;
            ApiVersion.TryParse( versionValue, out version );
            ApiVersion.TryParse( otherVersionValue, out otherVersion );

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
            ApiVersion version = null;
            ApiVersion otherVersion = null;
            ApiVersion.TryParse( versionValue, out version );
            ApiVersion.TryParse( otherVersionValue, out otherVersion );

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
            ApiVersion version = null;
            ApiVersion otherVersion = null;
            ApiVersion.TryParse( versionValue, out version );
            ApiVersion.TryParse( otherVersionValue, out otherVersion );

            // act
            var result = version >= otherVersion;

            // assert
            result.Should().Be( expected );
        }
    }
}