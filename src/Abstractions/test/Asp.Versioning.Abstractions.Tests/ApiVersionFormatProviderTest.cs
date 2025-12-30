// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

using System.Globalization;

#if NETFRAMEWORK
using DateOnly = System.DateTime;
#endif
using static System.Globalization.CultureInfo;

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
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_allow_null_or_empty_format_string( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = new ApiVersion( 1, 0 );
        var expected = new[] { apiVersion.ToString(), apiVersion.ToString() };

        // act
        var actual = new[]
        {
            provider.Format( null, apiVersion, CurrentCulture ),
            provider.Format( string.Empty, apiVersion, CurrentCulture ),
        };

        // assert
        actual.Should().Equal( expected );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_return_full_formatted_string_without_optional_components( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = ApiVersionParser.Default.Parse( "2017-05-01.1-Beta" );

        // act
        var format = provider.Format( "F", apiVersion, CurrentCulture );

        // assert
        format.Should().Be( "2017-05-01.1-Beta" );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_return_full_formatted_string_with_optional_components( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = ApiVersionParser.Default.Parse( "2017-05-01.1-Beta" );

        // act
        var format = provider.Format( "FF", apiVersion, CurrentCulture );

        // assert
        format.Should().Be( "2017-05-01.1.0-Beta" );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_return_original_string_format_when_argument_cannot_be_formatted( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var value = new object();
        var expected = new string[] { "d", value.ToString() };

        // act
        var actual = new[] { provider.Format( "d", null, CurrentCulture ), provider.Format( "d", value, CurrentCulture ) };

        // assert
        actual.Should().Equal( expected );
    }

    [Theory]
    [MemberData( nameof( MalformedLiteralStringsData ) )]
    public void format_should_not_allow_malformed_literal_string( FormatProviderKind kind, string malformedFormat )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = new ApiVersion( new DateOnly( 2017, 5, 1 ) );

        // act
        Action format = () => provider.Format( malformedFormat, apiVersion, null );

        // assert
        format.Should().Throw<FormatException>();
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( GroupVersionFormatData ) )]
    public void format_should_return_formatted_group_version_string( FormatProviderKind kind, string format )
    {
        // arrange
        var provider = GetProvider( kind );
        var groupVersion = new DateOnly( 2017, 5, 1 );
        var apiVersion = new ApiVersion( groupVersion );
        var expected = groupVersion.ToString( format, CurrentCulture );

        // act
        var actual = provider.Format( format, apiVersion, CurrentCulture );

        // assert
        actual.Should().Be( expected );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_return_formatted_minor_version_string( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = new ApiVersion( 2, 5 );

        // act
        var minorVersion = provider.Format( "v", apiVersion, CurrentCulture );

        // assert
        minorVersion.Should().Be( "5" );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_return_formatted_major_version_string( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = new ApiVersion( 2, 5 );

        // act
        var majorVersion = provider.Format( "V", apiVersion, CurrentCulture );

        // assert
        majorVersion.Should().Be( "2" );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_return_formatted_major_and_minor_version_string( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = new ApiVersion( 2, 0 );

        // act
        var minorVersion = provider.Format( "VV", apiVersion, CurrentCulture );

        // assert
        minorVersion.Should().Be( "2.0" );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_return_formatted_short_version_string( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = new ApiVersion( 2, 0 );

        // act
        var minorVersion = provider.Format( "VVV", apiVersion, CurrentCulture );

        // assert
        minorVersion.Should().Be( "2" );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_return_formatted_long_version_string( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = ApiVersionParser.Default.Parse( "1-RC" );

        // act
        var minorVersion = provider.Format( "VVVV", apiVersion, CurrentCulture );

        // assert
        minorVersion.Should().Be( "1.0-RC" );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( FormatProvidersData ) )]
    public void format_should_return_formatted_status_string( FormatProviderKind kind )
    {
        // arrange
        var provider = GetProvider( kind );
        var apiVersion = new ApiVersion( 2, 5, "Beta" );

        // act
        var status = provider.Format( "S", apiVersion, CurrentCulture );

        // assert
        status.Should().Be( "Beta" );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( PaddedMinorVersionFormatData ) )]
    public void format_should_return_formatted_minor_version_with_padding_string( FormatProviderKind kind, string format )
    {
        // arrange
        var provider = GetProvider( kind );
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
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( PaddedMajorVersionFormatData ) )]
    public void format_should_return_formatted_major_version_with_padding_string( FormatProviderKind kind, string format )
    {
        // arrange
        var provider = GetProvider( kind );
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
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( CustomFormatData ) )]
    public void format_should_return_custom_format_string( FormatProviderKind kind, string format, string expected )
    {
        // arrange
        var provider = GetProvider( kind );
        var groupVersion = new DateOnly( 2017, 5, 1 );
        var apiVersion = new ApiVersion( groupVersion, 1, 0, "Beta" );

        // act
        var actual = provider.Format( format, apiVersion, CurrentCulture );

        // assert
        actual.Should().Be( expected );
    }

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( StringCustomFormatData ) )]
    public void string_format_should_return_custom_format_string(
        FormatProviderKind kind,
        string format,
        string expected )
    {
        // arrange
        var provider = GetProvider( kind );
        var groupVersion = new DateOnly( 2017, 5, 1 );
        var apiVersion = new ApiVersion( groupVersion, 1, 0, "Beta" );

        // act
        var actual = string.Format( provider, format, apiVersion );

        // assert
        actual.Should().Be( expected );
    }

#pragma warning disable xUnit1045

    [Theory]
    [AssumeCulture( "en-us" )]
    [MemberData( nameof( MultipleFormatParameterData ) )]
    public void format_should_return_formatted_string_with_multiple_parameters(
        FormatProviderKind kind,
        string format,
        object secondArgument,
        string expected )
    {
        // arrange
        var provider = GetProvider( kind );
        var groupVersion = new DateOnly( 2017, 5, 1 );
        var apiVersion = new ApiVersion( groupVersion, 1, 0, "Beta" );
        var args = new object[] { apiVersion, secondArgument };

        // act
        var status = string.Format( provider, format, args );

        // assert
        status.Should().Be( expected );
    }

#pragma warning restore xUnit1045

    [Fact]
    [AssumeCulture( "en-us" )]
    public void format_should_return_formatted_string_with_escape_sequence()
    {
        // arrange
        var groupVersion = new DateOnly( 2017, 5, 1 );
        var apiVersion = new ApiVersion( groupVersion, 1, 0, "Beta" );
        var provider = new ApiVersionFormatProvider();

        // act
        var result = provider.Format( "VV '('\\'yy')'", apiVersion, CurrentCulture );

        // assert
        result.Should().Be( "1.0 ('17)" );
    }

    /// <summary>
    /// Represents the supported test format providers.
    /// </summary>
    public enum FormatProviderKind
    {
        /// <summary>
        /// <see cref="ApiVersionFormatProvider"/>.
        /// </summary>
        Default,

        /// <summary>
        /// <see cref="ApiVersionFormatProvider(DateTimeFormatInfo)"/>.
        /// </summary>
        DateTime,

        /// <summary>
        /// <see cref="ApiVersionFormatProvider(Calendar)"/>.
        /// </summary>
        Calendar,

        /// <summary>
        /// <see cref="ApiVersionFormatProvider(DateTimeFormatInfo, Calendar)"/>.
        /// </summary>
        DateTimeAndCalendar,
    }

    private static ApiVersionFormatProvider GetProvider( FormatProviderKind provider ) =>
        provider switch
        {
            FormatProviderKind.DateTime => new( DateTimeFormatInfo.CurrentInfo ),
            FormatProviderKind.Calendar => new( new GregorianCalendar() ),
            FormatProviderKind.DateTimeAndCalendar => new( DateTimeFormatInfo.CurrentInfo, new GregorianCalendar() ),
            _ => new ApiVersionFormatProvider(),
        };

    public static TheoryData<FormatProviderKind> FormatProvidersData =>
        [
            FormatProviderKind.Default,
            FormatProviderKind.DateTime,
            FormatProviderKind.Calendar,
            FormatProviderKind.DateTimeAndCalendar,
        ];

    public static TheoryData<FormatProviderKind, string> MalformedLiteralStringsData
    {
        get
        {
            var data = new TheoryData<FormatProviderKind, string>();

            foreach ( var provider in FormatProvidersData )
            {
                data.Add( provider, "'MM-dd-yyyy" );
                data.Add( provider, "MM-dd-yyyy'" );
                data.Add( provider, "\"MM-dd-yyyy" );
                data.Add( provider, "MM-dd-yyyy\"" );
            }

            return data;
        }
    }

    public static TheoryData<FormatProviderKind, string> GroupVersionFormatData
    {
        get
        {
            var data = new TheoryData<FormatProviderKind, string>();
            var formats = new[] { "%d", "dd", "ddd", "dddd", "%M", "MM", "MMM", "MMMM", "%y", "yy", "yyy", "yyyy" };

            foreach ( var provider in FormatProvidersData )
            {
                foreach ( var format in formats )
                {
                    data.Add( provider, format );
                }
            }

            return data;
        }
    }

    public static TheoryData<FormatProviderKind, string> PaddedMinorVersionFormatData
    {
        get
        {
            var data = new TheoryData<FormatProviderKind, string>();
            var formats = new[] { "p", "p0", "p1", "p2", "p3" };

            foreach ( var provider in FormatProvidersData )
            {
                foreach ( var format in formats )
                {
                    data.Add( provider, format );
                }
            }

            return data;
        }
    }

    public static TheoryData<FormatProviderKind, string> PaddedMajorVersionFormatData
    {
        get
        {
            var data = new TheoryData<FormatProviderKind, string>();
            var formats = new[] { "P", "P0", "P1", "P2", "P3" };

            foreach ( var provider in FormatProvidersData )
            {
                foreach ( var format in formats )
                {
                    data.Add( provider, format );
                }
            }

            return data;
        }
    }

    public static TheoryData<FormatProviderKind, string, string> CustomFormatData
    {
        get
        {
            var data = new TheoryData<FormatProviderKind, string, string>();

            foreach ( var provider in FormatProvidersData )
            {
                data.Add( provider, "'v'F", "v2017-05-01.1.0-Beta" );
                data.Add( provider, "'v'FF", "v2017-05-01.1.0-Beta" );
                data.Add( provider, "'v'V", "v1" );
                data.Add( provider, "'v'VV", "v1.0" );
                data.Add( provider, "V'.'v", "1.0" );
                data.Add( provider, "P.p", "01.00" );
                data.Add( provider, "'Group:' G, 'Version:' V.v, 'Status:' S", "Group: 2017-05-01, Version: 1.0, Status: Beta" );
                data.Add( provider, "'Group:' yyyy-MM-dd, 'Version:' V.v, 'Status:' S", "Group: 2017-05-01, Version: 1.0, Status: Beta" );
            }

            return data;
        }
    }

    public static TheoryData<FormatProviderKind, string, string> StringCustomFormatData
    {
        get
        {
            var data = new TheoryData<FormatProviderKind, string, string>();

            foreach ( var provider in FormatProvidersData )
            {
                data.Add( provider, "v{0:F}", "v2017-05-01.1.0-Beta" );
                data.Add( provider, "v{0:FF}", "v2017-05-01.1.0-Beta" );
                data.Add( provider, "v{0:V}", "v1" );
                data.Add( provider, "v{0:VV}", "v1.0" );
                data.Add( provider, "{0:V}.{0:v}", "1.0" );
                data.Add( provider, "{0:P3}.{0:p3}", "001.000" );
                data.Add( provider, "{0:\"Group:\" G, \"Version:\" V.v, \"Status:\" S}", "Group: 2017-05-01, Version: 1.0, Status: Beta" );
                data.Add( provider, "{0:\"Group:\" yyyy-MM-dd, \"Version:\" V.v, \"Status:\" S}", "Group: 2017-05-01, Version: 1.0, Status: Beta" );
            }

            return data;
        }
    }

    public static TheoryData<FormatProviderKind, string, object, string> MultipleFormatParameterData
    {
        get
        {
            var data = new TheoryData<FormatProviderKind, string, object, string>();

            foreach ( var provider in FormatProvidersData )
            {
                data.Add( provider, "{0:yyyy}->{0:MM}->{0:dd} ({1})", "Group", "2017->05->01 (Group)" );
                data.Add( provider, "{0:'v'VV}, Deprecated = {1}", false, "v1.0, Deprecated = False" );
                data.Add( provider, "Major = {0:V}, Minor = {0:v}, Iteration = {1:N1}", 1, "Major = 1, Minor = 0, Iteration = 1.0" );
                data.Add( provider, "Major\t| Minor\t| Iteration\n{0:P}\t\t| {0:p}\t| {1:N1}", 1, "Major\t| Minor\t| Iteration\n01\t\t| 00\t| 1.0" );
            }

            return data;
        }
    }
}