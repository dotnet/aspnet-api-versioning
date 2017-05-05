namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Xunit;
    using static System.Globalization.CultureInfo;
    using static System.String;

    public class ApiVersionFormatProviderTest
    {
        [Fact]
        public void get_format_should_return_null_for_unsupported_format_type()
        {
            // arrange
            var formatType = typeof( object );
            var provider = new ApiVersionFormatProvider();

            // act
            var format = provider.GetFormat( formatType );

            // assert
            format.Should().BeNull();
        }

        [Fact]
        public void get_format_should_return_expected_format_provider()
        {
            // arrange
            var formatType = typeof( ICustomFormatter );
            var provider = new ApiVersionFormatProvider();

            // act
            var format = provider.GetFormat( formatType );

            // assert
            format.Should().BeSameAs( provider );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_allow_null_or_empty_format_string( ApiVersionFormatProvider provider )
        {
            // arrange
            var apiVersion = new ApiVersion( 1, 0 );
            var expected = new[] { apiVersion.ToString(), apiVersion.ToString() };

            // act
            var actual = new[] { provider.Format( null, apiVersion, CurrentCulture ), provider.Format( Empty, apiVersion, CurrentCulture ) };

            // assert
            actual.Should().Equal( expected );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_return_full_formatted_string_without_optional_components( ApiVersionFormatProvider provider )
        {
            // arrange
            var apiVersion = ApiVersion.Parse( "2017-05-01.1-Beta" );

            // act
            var format = provider.Format( "F", apiVersion, CurrentCulture );

            // assert
            format.Should().Be( "2017-05-01.1-Beta" );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_return_full_formatted_string_with_optional_components( ApiVersionFormatProvider provider )
        {
            // arrange
            var apiVersion = ApiVersion.Parse( "2017-05-01.1-Beta" );

            // act
            var format = provider.Format( "FF", apiVersion, CurrentCulture );

            // assert
            format.Should().Be( "2017-05-01.1.0-Beta" );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_return_original_string_format_when_argument_cannot_be_formatted( ApiVersionFormatProvider provider )
        {
            // arrange
            var value = new object();
            var expected = new string[] { "d", value.ToString() };

            // act
            var actual = new[] { provider.Format( "d", null, CurrentCulture ), provider.Format( "d", value, CurrentCulture ) };

            // assert
            actual.Should().Equal( expected );
        }

        [Theory]
        [MemberData( nameof( MalformedLiteralStringsData ) )]
        public void format_should_not_allow_malformed_literal_string( ApiVersionFormatProvider provider, string malformedFormat )
        {
            // arrange
            var apiVersion = new ApiVersion( new DateTime( 2017, 5, 1 ) );

            // act
            Action format = () => provider.Format( malformedFormat, apiVersion, null );

            // assert
            format.ShouldThrow<FormatException>();
        }

        [Theory]
        [MemberData( nameof( GroupVersionFormatData ) )]
        public void format_should_return_formatted_group_version_string( ApiVersionFormatProvider provider, string format )
        {
            // arrange
            var groupVersion = new DateTime( 2017, 5, 1 );
            var apiVersion = new ApiVersion( groupVersion );
            var expected = groupVersion.ToString( format, CurrentCulture );

            // act
            var actual = provider.Format( format, apiVersion, CurrentCulture );

            // assert
            actual.Should().Be( expected );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_return_formatted_minor_version_string( ApiVersionFormatProvider provider )
        {
            // arrange
            var apiVersion = new ApiVersion( 2, 5 );

            // act
            var minorVersion = provider.Format( "v", apiVersion, CurrentCulture );

            // assert
            minorVersion.Should().Be( "5" );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_return_formatted_major_version_string( ApiVersionFormatProvider provider )
        {
            // arrange
            var apiVersion = new ApiVersion( 2, 5 );

            // act
            var majorVersion = provider.Format( "V", apiVersion, CurrentCulture );

            // assert
            majorVersion.Should().Be( "2" );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_return_formatted_major_and_minor_version_string( ApiVersionFormatProvider provider )
        {
            // arrange
            var apiVersion = new ApiVersion( 2, 0 );

            // act
            var minorVersion = provider.Format( "VV", apiVersion, CurrentCulture );

            // assert
            minorVersion.Should().Be( "2.0" );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_return_formatted_short_version_string( ApiVersionFormatProvider provider )
        {
            // arrange
            var apiVersion = new ApiVersion( 2, 0 );

            // act
            var minorVersion = provider.Format( "VVV", apiVersion, CurrentCulture );

            // assert
            minorVersion.Should().Be( "2" );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_return_formatted_long_version_string( ApiVersionFormatProvider provider )
        {
            // arrange
            var apiVersion = ApiVersion.Parse( "1-RC" );

            // act
            var minorVersion = provider.Format( "VVVV", apiVersion, CurrentCulture );

            // assert
            minorVersion.Should().Be( "1.0-RC" );
        }

        [Theory]
        [MemberData( nameof( FormatProvidersData ) )]
        public void format_should_return_formatted_status_string( ApiVersionFormatProvider provider )
        {
            // arrange
            var apiVersion = new ApiVersion( 2, 5, "Beta" );

            // act
            var status = provider.Format( "S", apiVersion, CurrentCulture );

            // assert
            status.Should().Be( "Beta" );
        }

        [Theory]
        [MemberData( nameof( PaddedMinorVersionFormatData ) )]
        public void format_should_return_formatted_minor_version_with_padding_string( ApiVersionFormatProvider provider, string format )
        {
            // arrange
            var numberFormat = format.Replace( "p", "D" );
            var apiVersion = new ApiVersion( 2, 5 );

            if ( numberFormat == "D" )
            {
                numberFormat += "2";
            }

            // act
            var minorVersion = provider.Format( format, apiVersion, CurrentCulture );

            // assert
            minorVersion.Should().Be( apiVersion.MinorVersion.Value.ToString( numberFormat, CurrentCulture ) );
        }

        [Theory]
        [MemberData( nameof( PaddedMajorVersionFormatData ) )]
        public void format_should_return_formatted_major_version_with_padding_string( ApiVersionFormatProvider provider, string format )
        {
            // arrange
            var numberFormat = format.Replace( "P", "D" );
            var apiVersion = new ApiVersion( 2, 5 );

            if ( numberFormat == "D" )
            {
                numberFormat += "2";
            }

            // act
            var majorVersion = provider.Format( format, apiVersion, CurrentCulture );

            // assert
            majorVersion.Should().Be( apiVersion.MajorVersion.Value.ToString( numberFormat, CurrentCulture ) );
        }

        [Theory]
        [MemberData( nameof( CustomFormatData ) )]
        public void format_should_return_custom_format_string( Func<ApiVersion, string> format, string expected )
        {
            // arrange
            var groupVersion = new DateTime( 2017, 5, 1 );
            var apiVersion = new ApiVersion( groupVersion, 1, 0, "Beta" );

            // act
            var actual = format( apiVersion );

            // assert
            actual.Should().Be( expected );
        }

        [Theory]
        [MemberData( nameof( MultipleFormatParameterData ) )]
        public void format_should_return_formatted_string_with_multiple_parameters( ApiVersionFormatProvider provider, string format, object secondArgument, string expected )
        {
            // arrange
            var groupVersion = new DateTime( 2017, 5, 1 );
            var apiVersion = new ApiVersion( groupVersion, 1, 0, "Beta" );
            var args = new object[] { apiVersion, secondArgument };

            // act
            var status = Format( provider, format, args );

            // assert
            status.Should().Be( expected );
        }

        [Fact]
        public void format_should_return_formatted_string_with_escape_sequence()
        {
            // arrange
            var groupVersion = new DateTime( 2017, 5, 1 );
            var apiVersion = new ApiVersion( groupVersion, 1, 0, "Beta" );
            var provider = new ApiVersionFormatProvider();

            // act
            var result = provider.Format( "VV '('\\'yy')'", apiVersion, CurrentCulture );

            // assert
            result.Should().Be( "1.0 ('17)" );
        }

        public static IEnumerable<object[]> FormatProvidersData
        {
            get
            {
                yield return new object[] { new ApiVersionFormatProvider() };
                yield return new object[] { new ApiVersionFormatProvider( DateTimeFormatInfo.CurrentInfo ) };
                yield return new object[] { new ApiVersionFormatProvider( new GregorianCalendar() ) };
                yield return new object[] { new ApiVersionFormatProvider( DateTimeFormatInfo.CurrentInfo, new GregorianCalendar() ) };
            }
        }

        public static IEnumerable<object[]> MalformedLiteralStringsData
        {
            get
            {
                foreach ( var provider in FormatProvidersData.Select( d => d[0] ).Cast<ApiVersionFormatProvider>() )
                {
                    yield return new object[] { provider, "'MM-dd-yyyy" };
                    yield return new object[] { provider, "MM-dd-yyyy'" };
                    yield return new object[] { provider, "\"MM-dd-yyyy" };
                    yield return new object[] { provider, "MM-dd-yyyy\"" };
                }
            }
        }

        public static IEnumerable<object[]> GroupVersionFormatData
        {
            get
            {
                var formats = new[] { "%d", "dd", "ddd", "dddd", "%M", "MM", "MMM", "MMMM", "%y", "yy", "yyy", "yyyy" };

                foreach ( var provider in FormatProvidersData.Select( d => d[0] ).Cast<ApiVersionFormatProvider>() )
                {
                    foreach ( var format in formats )
                    {
                        yield return new object[] { provider, format };
                    }
                }
            }
        }

        public static IEnumerable<object[]> PaddedMinorVersionFormatData
        {
            get
            {
                var formats = new[] { "p", "p0", "p1", "p2", "p3" };

                foreach ( var provider in FormatProvidersData.Select( d => d[0] ).Cast<ApiVersionFormatProvider>() )
                {
                    foreach ( var format in formats )
                    {
                        yield return new object[] { provider, format };
                    }
                }
            }
        }

        public static IEnumerable<object[]> PaddedMajorVersionFormatData
        {
            get
            {
                var formats = new[] { "P", "P0", "P1", "P2", "P3" };

                foreach ( var provider in FormatProvidersData.Select( d => d[0] ).Cast<ApiVersionFormatProvider>() )
                {
                    foreach ( var format in formats )
                    {
                        yield return new object[] { provider, format };
                    }
                }
            }
        }

        public static IEnumerable<object[]> CustomFormatData
        {
            get
            {
                foreach ( var provider in FormatProvidersData.Select( d => d[0] ).Cast<ApiVersionFormatProvider>() )
                {
                    yield return new object[] { new Func<ApiVersion, string>( v => provider.Format( "'v'F", v, CurrentCulture ) ), "v2017-05-01.1.0-Beta" };
                    yield return new object[] { new Func<ApiVersion, string>( v => provider.Format( "'v'FF", v, CurrentCulture ) ), "v2017-05-01.1.0-Beta" };
                    yield return new object[] { new Func<ApiVersion, string>( v => Format( provider, "v{0:F}", v ) ), "v2017-05-01.1.0-Beta" };
                    yield return new object[] { new Func<ApiVersion, string>( v => Format( provider, "v{0:FF}", v ) ), "v2017-05-01.1.0-Beta" };
                    yield return new object[] { new Func<ApiVersion, string>( v => provider.Format( "'v'V", v, CurrentCulture ) ), "v1" };
                    yield return new object[] { new Func<ApiVersion, string>( v => provider.Format( "'v'VV", v, CurrentCulture ) ), "v1.0" };
                    yield return new object[] { new Func<ApiVersion, string>( v => Format( provider, "v{0:V}", v ) ), "v1" };
                    yield return new object[] { new Func<ApiVersion, string>( v => Format( provider, "v{0:VV}", v ) ), "v1.0" };
                    yield return new object[] { new Func<ApiVersion, string>( v => provider.Format( "V'.'v", v, CurrentCulture ) ), "1.0" };
                    yield return new object[] { new Func<ApiVersion, string>( v => Format( provider, "{0:V}.{0:v}", v ) ), "1.0" };
                    yield return new object[] { new Func<ApiVersion, string>( v => provider.Format( "P.p", v, CurrentCulture ) ), "01.00" };
                    yield return new object[] { new Func<ApiVersion, string>( v => Format( provider, "{0:P3}.{0:p3}", v ) ), "001.000" };
                    yield return new object[] { new Func<ApiVersion, string>( v => provider.Format( "'Group:' G, 'Version:' V.v, 'Status:' S", v, CurrentCulture ) ), "Group: 2017-05-01, Version: 1.0, Status: Beta" };
                    yield return new object[] { new Func<ApiVersion, string>( v => provider.Format( "'Group:' yyyy-MM-dd, 'Version:' V.v, 'Status:' S", v, CurrentCulture ) ), "Group: 2017-05-01, Version: 1.0, Status: Beta" };
                    yield return new object[] { new Func<ApiVersion, string>( v => Format( provider, "{0:\"Group:\" G, \"Version:\" V.v, \"Status:\" S}", v ) ), "Group: 2017-05-01, Version: 1.0, Status: Beta" };
                    yield return new object[] { new Func<ApiVersion, string>( v => Format( provider, "{0:\"Group:\" yyyy-MM-dd, \"Version:\" V.v, \"Status:\" S}", v ) ), "Group: 2017-05-01, Version: 1.0, Status: Beta" };
                }
            }
        }

        public static IEnumerable<object[]> MultipleFormatParameterData
        {
            get
            {
                foreach ( var provider in FormatProvidersData.Select( d => d[0] ).Cast<ApiVersionFormatProvider>() )
                {
                    yield return new object[] { provider, "{0:yyyy}->{0:MM}->{0:dd} ({1})", "Group", "2017->05->01 (Group)" };
                    yield return new object[] { provider, "{0:'v'VV}, Deprecated = {1}", false, "v1.0, Deprecated = False" };
                    yield return new object[] { provider, "Major = {0:V}, Minor = {0:v}, Iteration = {1:N1}", 1, "Major = 1, Minor = 0, Iteration = 1.0" };
                    yield return new object[] { provider, "Major\t| Minor\t| Iteration\n{0:P}\t\t| {0:p}\t| {1:N1}", 1, "Major\t| Minor\t| Iteration\n01\t\t| 00\t| 1.0" };
                }
            }
        }
    }
}