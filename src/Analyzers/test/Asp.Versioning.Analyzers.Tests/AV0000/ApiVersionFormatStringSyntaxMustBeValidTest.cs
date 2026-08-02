// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.AV0000;

public class ApiVersionFormatStringSyntaxMustBeValidTest
{
    private const string AV0009 = nameof( AV0009 );
    private const string AV0010 = nameof( AV0010 );

    private static readonly ApiVersion Sample = ApiVersionParser.Default.Parse( "2017-05-01.1.5-RC" );

    [Theory]
    [InlineData( "" )]
    [InlineData( "F" )]
    [InlineData( "FF" )]
    [InlineData( "G" )]
    [InlineData( "GG" )]
    [InlineData( "y" )]
    [InlineData( "yyyy" )]
    [InlineData( "yyyyy" )]
    [InlineData( "MM" )]
    [InlineData( "MMMM" )]
    [InlineData( "dd" )]
    [InlineData( "dddd" )]
    [InlineData( "v" )]
    [InlineData( "V" )]
    [InlineData( "VV" )]
    [InlineData( "VVV" )]
    [InlineData( "VVVV" )]
    [InlineData( "S" )]
    [InlineData( "p" )]
    [InlineData( "p3" )]
    [InlineData( "p99" )]
    [InlineData( "P" )]
    [InlineData( "PPPP" )]
    [InlineData( "V.v" )]
    [InlineData( "'v'V" )]
    [InlineData( "%V" )]
    public async Task analyzer_should_not_report_valid_format( string format )
    {
        // arrange
        var source = Formatted( format );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
        Throws( format ).Should().BeFalse( "the analyzer must agree with ApiVersionFormatProvider" );
    }

    [Theory]
    [InlineData( "'unterminated" )]
    [InlineData( "\"unterminated" )]
    [InlineData( "MM-dd-yyyy'" )]
    [InlineData( "p100" )]
    [InlineData( "P100" )]
    [InlineData( "p256" )]
    [InlineData( "p2147483648" )]
    [InlineData( "p99999999999999999999" )]
    public async Task analyzer_should_report_malformed_format( string format )
    {
        // arrange
        var source = Formatted( format );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0009 );
        Throws( format ).Should().BeTrue( "AV0009 is only for a format that fails at run time" );
    }

    [Theory]
    [InlineData( "FFF" )]
    [InlineData( "GGG" )]
    [InlineData( "SS" )]
    [InlineData( "vv" )]
    [InlineData( "pp" )]
    [InlineData( "MMMMM" )]
    [InlineData( "ddddd" )]
    [InlineData( "VVVVV" )]
    [InlineData( "PPPPP" )]
    public async Task analyzer_should_report_repeated_specifier( string format )
    {
        // arrange
        var source = Formatted( format );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0010 );
        Throws( format ).Should().BeFalse( "AV0010 is for a format that silently misbehaves" );
    }

    [Fact]
    public async Task analyzer_should_report_repeated_specifier_as_a_warning()
    {
        // arrange
        // an over-repeated specifier still formats, so it cannot fail the build the way AV0009 does
        var source = Formatted( "vv" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;

        diagnostic.Severity.Should().Be( DiagnosticSeverity.Warning );
        diagnostic.GetMessage().Should().Contain( "'v'" ).And.Contain( "1" ).And.Contain( "2" );
    }

    [Fact]
    public async Task analyzer_should_report_malformed_format_as_an_error()
    {
        // arrange
        var source = Formatted( "p100" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;

        diagnostic.Severity.Should().Be( DiagnosticSeverity.Error );
        diagnostic.GetMessage().Should().Contain( "100" ).And.Contain( "99" );
    }

    [Fact]
    public async Task analyzer_should_report_every_problem_in_a_format()
    {
        // arrange
        var source = Formatted( "VVVVV-vv" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Select( diagnostic => diagnostic.Id ).Should().BeEquivalentTo( [AV0010, AV0010] );
    }

    [Fact]
    public async Task analyzer_should_report_format_passed_to_to_string_with_provider()
    {
        // arrange
        var source = """
            using System;
            using Asp.Versioning;

            public class Formatter
            {
                public string Format( ApiVersion version, IFormatProvider provider ) =>
                    version.ToString( "vv", provider );
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0010 );
    }

    [Fact]
    public async Task analyzer_should_report_format_passed_to_try_format()
    {
        // arrange
        var source = """
            using System;
            using Asp.Versioning;

            public class Formatter
            {
                public bool Format( ApiVersion version, Span<char> destination ) =>
                    version.TryFormat( destination, out _, "p100", null );
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0009 );
    }

    [Fact]
    public async Task analyzer_should_report_format_passed_to_format_provider()
    {
        // arrange
        var source = """
            using Asp.Versioning;

            public class Formatter
            {
                public string Format( ApiVersion version ) =>
                    ApiVersionFormatProvider.CurrentCulture.Format( "vv", version, null );
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0010 );
    }

    [Fact]
    public async Task analyzer_should_not_report_format_known_only_at_run_time()
    {
        // arrange
        var source = """
            using Asp.Versioning;

            public class Formatter
            {
                public string Format( ApiVersion version, string format ) => version.ToString( format );
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_an_api_version_as_a_format()
    {
        // arrange
        // "1.0" is a valid API version but reads as a format of literal characters, and the two
        // syntaxes must not be confused for one another
        var source = """
            using Asp.Versioning;

            [ApiVersion( "1.0" )]
            public class Controller
            {
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    private static bool Throws( string format )
    {
        try
        {
            Sample.ToString( format );
            return false;
        }
        catch ( FormatException )
        {
            return true;
        }
    }

    private static string Formatted( string format ) =>
        $$"""
        using Asp.Versioning;

        public class Formatter
        {
            public string Format( ApiVersion version ) => version.ToString( {{AnalyzerVerifier.Literal( format )}} );
        }
        """;
}