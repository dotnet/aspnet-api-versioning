// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning;

public partial class ApiVersionTest
{
    [Theory]
    [MemberData( nameof( FormatData ) )]
    [AssumeCulture( "en-us" )]
    public void try_format_format_should_return_expected_string( string format, string text, string formattedString )
    {
        // arrange
        var apiVersion = ApiVersionParser.Default.Parse( text );
        Span<char> buffer = stackalloc char[32];

        // act
        var result = apiVersion.TryFormat( buffer, out var written, format, default );

        // assert
        result.Should().BeTrue();
        buffer[..written].ToString().Should().Be( formattedString );
    }
}