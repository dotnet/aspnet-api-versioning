// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.AV0000;

public class ApiVersionRangeStringSyntaxMustBeValidTest
{
    private const string AV0002 = nameof( AV0002 );

    private const string Declarations = """
        using System;
        using System.Diagnostics.CodeAnalysis;

        [AttributeUsage( AttributeTargets.Class )]
        public sealed class RangedAttribute : Attribute
        {
            public RangedAttribute( [StringSyntax( "ApiVersionRange" )] string rule ) { }

            [StringSyntax( "ApiVersionRange" )]
            public string Sunset { get; set; }
        }

        public static class Api
        {
            public static void Restrict( [StringSyntax( "ApiVersionRange" )] string rule ) { }
        }
        """;

    [Theory]
    [InlineData( "1" )]
    [InlineData( "1.0" )]
    [InlineData( "[1]" )]
    [InlineData( "[1.0]" )]
    [InlineData( "1.0-beta" )]
    [InlineData( "2013-08-06" )]
    [InlineData( "[1.0,)" )]
    [InlineData( "(1.0,)" )]
    [InlineData( "(,1.0]" )]
    [InlineData( "(,1.0)" )]
    [InlineData( "[1.0,2.0]" )]
    [InlineData( "(1.0,2.0)" )]
    [InlineData( "[1.0,2.0)" )]
    [InlineData( "(1.0,2.0]" )]
    [InlineData( "[1.0-beta,)" )]
    [InlineData( "[2013-08-06,2013-09-01)" )]
    [InlineData( "[1.0 , 2.0]" )]
    public async Task analyzer_should_not_report_valid_api_version_range( string rule )
    {
        // arrange
        var source = Ranged( rule );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
        CanParse( rule ).Should().BeTrue( "the analyzer must agree with ApiVersionRange" );
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "()" )]
    [InlineData( "[]" )]
    [InlineData( "," )]
    [InlineData( "(,)" )]
    [InlineData( "[,]" )]
    [InlineData( "[,)" )]
    [InlineData( "(1.0)" )]
    [InlineData( "[1.0)" )]
    [InlineData( "(1.0]" )]
    [InlineData( "(1.0," )]
    [InlineData( "1.0,2.0" )]
    [InlineData( "[1.0,2.0" )]
    [InlineData( "1.0,2.0]" )]
    [InlineData( "[1.0,)]" )]
    [InlineData( "[bogus,)" )]
    [InlineData( "(,bogus]" )]
    [InlineData( "[1.0,,2.0]" )]
    [InlineData( "[[1.0,2.0]]" )]
    [InlineData( "[1.0,2.0]extra" )]
    [InlineData( "[ 1.0,2.0]" )]
    [InlineData( "[1.0,2.0 ]" )]
    [InlineData( " 1.0" )]
    [InlineData( "1.0 " )]
    public async Task analyzer_should_report_invalid_api_version_range( string rule )
    {
        // arrange
        var source = Ranged( rule );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0002 );
        CanParse( rule ).Should().BeFalse( "the analyzer must agree with ApiVersionRange" );
    }

    [Theory]
    [InlineData( "]1.0,2.0[" )]
    [InlineData( "]1.0,2.0]" )]
    [InlineData( "]1.0,2.0)" )]
    [InlineData( ")1.0,2.0[" )]
    [InlineData( ")1.0,2.0]" )]
    [InlineData( ")1.0,2.0)" )]
    [InlineData( "[1.0,2.0[" )]
    [InlineData( "[1.0,2.0(" )]
    [InlineData( "(1.0,2.0[" )]
    [InlineData( "(1.0,2.0(" )]
    public async Task analyzer_should_report_mismatched_bounds( string rule )
    {
        // arrange
        // only '[' and '(' are a lower bound and only ']' and ')' are an upper bound
        var source = Ranged( rule );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0002 );
        CanParse( rule ).Should().BeFalse( "the analyzer must agree with ApiVersionRange" );
    }

    [Fact]
    public async Task analyzer_should_report_api_version_range_at_argument_location()
    {
        // arrange
        var source = Ranged( "(1.0)" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "\"(1.0)\"" );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_range_in_expanded_params()
    {
        // arrange
        var source = """
            using Asp.Versioning;

            public class Model
            {
                [VisibleInApiVersion( "[1.0,)", "[2.0,3.0]", "(4.0)" )]
                public string Name { get; set; }
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0002 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_range_in_params_array()
    {
        // arrange
        var source = """
            using Asp.Versioning;

            public class Model
            {
                [VisibleInApiVersion( "[1.0,)", new[] { "(4.0)" } )]
                public string Name { get; set; }
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0002 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_range_in_attribute_property()
    {
        // arrange
        var source = Declared( """
            [Ranged( "[1.0,)", Sunset = "(2.0)" )]
            public class Controller
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0002 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_range_in_method_call()
    {
        // arrange
        var source = Declared( """
            public class Controller
            {
                public void Get() => Api.Restrict( "(1.0)" );
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0002 );
    }

    [Fact]
    public async Task analyzer_should_not_report_api_version_range_known_only_at_run_time()
    {
        // arrange
        var source = Declared( """
            public class Controller
            {
                public void Get( string rule ) => Api.Restrict( rule );
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_range_annotated_by_internal_attribute()
    {
        // arrange
        // .NET Standard builds compile an internal copy of StringSyntaxAttribute rather than using the
        // one from the BCL, so the annotation cannot be resolved as a type symbol from the compilation
        const string Backport = """
            using System;
            using System.Diagnostics.CodeAnalysis;

            namespace System.Diagnostics.CodeAnalysis
            {
                [AttributeUsage( AttributeTargets.Parameter )]
                internal sealed class StringSyntaxAttribute : Attribute
                {
                    public StringSyntaxAttribute( string syntax ) { }
                }
            }

            [AttributeUsage( AttributeTargets.Class )]
            public sealed class BackportedRangedAttribute : Attribute
            {
                public BackportedRangedAttribute( [StringSyntax( "ApiVersionRange" )] string rule ) { }
            }
            """;

        var library = AnalyzerVerifier.EmitAssembly( "BackportedRange", Backport );
        var source = """
            [BackportedRanged( "(1.0)" )]
            public class Controller
            {
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source, library );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0002 );
    }

    [Fact]
    public async Task analyzers_should_not_report_each_others_syntax()
    {
        // arrange
        // "[1.0,)" is a valid range but not a valid API version, and "1.0-beta" is a valid API
        // version that is also a valid range, so neither analyzer may act on the other's annotation
        var source = """
            using Asp.Versioning;

            [ApiVersion( "1.0-beta" )]
            public class Model
            {
                [VisibleInApiVersion( "[1.0,)" )]
                public string Name { get; set; }
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    private static bool CanParse( string rule )
    {
        try
        {
            ApiVersionRange.Parse( rule );
            return true;
        }
        catch ( FormatException )
        {
            return false;
        }
        catch ( ArgumentException )
        {
            return false;
        }
    }

    private static string Ranged( string rule ) =>
        $$"""
        using Asp.Versioning;

        public class Model
        {
            [VisibleInApiVersion( {{AnalyzerVerifier.Literal( rule )}} )]
            public string Name { get; set; }
        }
        """;

    private static string Declared( string code ) => Declarations + Environment.NewLine + code;
}