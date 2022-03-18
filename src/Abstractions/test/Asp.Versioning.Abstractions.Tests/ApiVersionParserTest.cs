// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public class ApiVersionParserTest
{
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
        var groupVersion = NewGroupVersion( groupVersionValue );

        // act
        var apiVersion = ApiVersionParser.Default.Parse( text );

        // assert
        apiVersion.Should().BeEquivalentTo(
            new
            {
                GroupVersion = groupVersion,
                MajorVersion = majorVersion,
                MinorVersion = minorVersion,
                Status = status,
            } );
    }

    [Theory]
    [InlineData( "Alpha1", "The specified API version is invalid." )]
    [InlineData( "1.1-Alpha-1", "The specified API version status 'Alpha-1' is invalid." )]
    [InlineData( "2013-02-29.1.0", "The specified API group version '2013-02-29' is invalid." )]
    public void parse_should_throw_format_exception_for_invalid_text( string text, string message )
    {
        // arrange

        // act
        var parse = () => ApiVersionParser.Default.Parse( text );

        // assert
        parse.Should().Throw<FormatException>().WithMessage( message );
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
        var groupVersion = NewGroupVersion( groupVersionValue );

        // act
        var result = ApiVersionParser.Default.TryParse( text, out var apiVersion );

        // assert
        result.Should().BeTrue();
        apiVersion.Should().BeEquivalentTo(
            new
            {
                GroupVersion = groupVersion,
                MajorVersion = majorVersion,
                MinorVersion = minorVersion,
                Status = status,
            } );
    }

    [Theory]
    [InlineData( "Alpha1" )]
    [InlineData( "1.1-Alpha-1" )]
    [InlineData( "2013-02-29.1.0" )]
    public void try_parse_should_return_false_when_text_is_invalid( string text )
    {
        // arrange

        // act
        var result = ApiVersionParser.Default.TryParse( text, out var apiVersion );

        // assert
        result.Should().BeFalse();
        apiVersion.Should().BeNull();
    }

#if NETFRAMEWORK
    private static DateTime? NewGroupVersion( string value ) => value is null ? null : new DateTime?( DateTime.Parse( value ) );
#else
    private static DateOnly? NewGroupVersion( string value ) => value is null ? null : new DateOnly?( DateOnly.Parse( value ) );
#endif
}