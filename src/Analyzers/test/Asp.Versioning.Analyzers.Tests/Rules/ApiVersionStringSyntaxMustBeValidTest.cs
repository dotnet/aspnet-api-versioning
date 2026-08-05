// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class ApiVersionStringSyntaxMustBeValidTest
{
    private const string AV0001 = nameof( AV0001 );

    private const string Declarations = """
        using System;
        using System.Diagnostics.CodeAnalysis;

        [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true )]
        public sealed class VersionedAttribute : Attribute
        {
            public VersionedAttribute( [StringSyntax( "ApiVersion" )] string version ) { }

            public VersionedAttribute( double version ) { }

            [StringSyntax( "ApiVersion" )]
            public string Sunset { get; set; }
        }

        [AttributeUsage( AttributeTargets.Class )]
        public sealed class VersionSetAttribute : Attribute
        {
            public VersionSetAttribute(
                [StringSyntax( "ApiVersion" )] string version,
                [StringSyntax( "ApiVersion" )] params string[] otherVersions ) { }
        }

        public static class Api
        {
            public static void Use( [StringSyntax( "ApiVersion" )] string version ) { }
        }
        """;

    [Theory]
    [InlineData( "1" )]
    [InlineData( "0" )]
    [InlineData( "1.0" )]
    [InlineData( "0.0" )]
    [InlineData( "01.0" )]
    [InlineData( "2147483647" )]
    [InlineData( "1.0-beta" )]
    [InlineData( "1.0-alpha.1" )]
    [InlineData( "1-" )]
    [InlineData( "1.0-" )]
    [InlineData( "2013-08-06" )]
    [InlineData( "2013-08-06-Alpha" )]
    [InlineData( "2013-08-06.1" )]
    [InlineData( "2013-08-06.1.1" )]
    [InlineData( "2013-08-06.1-Alpha" )]
    [InlineData( "2013-08-06.1.1-Alpha" )]
    public async Task analyzer_should_not_report_valid_api_version( string version )
    {
        // arrange
        var source = Versioned( version );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
        ApiVersionParser.Default
                        .TryParse( version, out _ )
                        .Should()
                        .BeTrue( "the analyzer must agree with ApiVersionParser" );
    }

    [Theory]
    [InlineData( "" )]
    [InlineData( "neutral" )]
    [InlineData( "Alpha1" )]
    [InlineData( "v1" )]
    [InlineData( "1_0" )]
    [InlineData( "1.0.0" )]
    [InlineData( "1." )]
    [InlineData( ".1" )]
    [InlineData( "--" )]
    [InlineData( "1.-1" )]
    [InlineData( "1.1-Alpha-1" )]
    [InlineData( "2147483648" )]
    [InlineData( "-1" )]
    [InlineData( "-1.0" )]
    [InlineData( "+1.0" )]
    [InlineData( " 1.0" )]
    [InlineData( "1.0 " )]
    [InlineData( "2013-02-29" )]
    [InlineData( "2025-13-45" )]
    [InlineData( "2013-08-06X" )]
    [InlineData( "2013-08-06." )]
    [InlineData( "2013-08-06-" )]
    public async Task analyzer_should_report_invalid_api_version( string version )
    {
        // arrange
        var source = Versioned( version );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
        ApiVersionParser.Default
                        .TryParse( version, out _ )
                        .Should()
                        .BeFalse( "the analyzer must agree with ApiVersionParser" );
    }

    [Fact]
    public async Task analyzer_should_report_api_version_at_argument_location()
    {
        // arrange
        var source = Versioned( "neutral" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "\"neutral\"" );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_in_attribute_property()
    {
        // arrange
        var source = Declared( """
            [Versioned( "1.0", Sunset = "1.x" )]
            public class Controller
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_in_expanded_params()
    {
        // arrange
        var source = Declared( """
            [VersionSet( "1.0", "2.0", "bogus" )]
            public class Controller
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_in_params_array()
    {
        // arrange
        var source = Declared( """
            [VersionSet( "1.0", new[] { "2.0", "bogus" } )]
            public class Controller
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_in_method_call()
    {
        // arrange
        var source = Declared( """
            public class Controller
            {
                public void Get() => Api.Use( "1.x" );
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_in_named_argument()
    {
        // arrange
        var source = Declared( """
            public class Controller
            {
                public void Get() => Api.Use( version: "1.x" );
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_in_object_creation()
    {
        // arrange
        var source = Declared( """
            public class Controller
            {
                public void Get()
                {
                    var attribute = new VersionedAttribute( "1.x" );
                }
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_in_implicit_object_creation()
    {
        // arrange
        var source = Declared( """
            public class Controller
            {
                public void Get()
                {
                    VersionedAttribute attribute = new( "1.x" );
                }
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_from_constant()
    {
        // arrange
        var source = Declared( """
            public class Controller
            {
                private const string Version = "1.x";

                [Versioned( Version )]
                public void Get() { }
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
    }

    [Fact]
    public async Task analyzer_should_not_report_api_version_known_only_at_run_time()
    {
        // arrange
        var source = Declared( """
            public class Controller
            {
                public void Get( string version ) => Api.Use( version );
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_unannotated_parameter()
    {
        // arrange
        var source = Declared( """
            [Versioned( 1.0 )]
            public class Controller
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_declared_in_metadata()
    {
        // arrange
        var source = """
            using Asp.Versioning;

            [ApiVersion( "1.0" )]
            [ApiVersion( "neutral" )]
            [AdvertiseApiVersions( "1.0", "bogus" )]
            public class Controller
            {
                [MapToApiVersion( "2.x" )]
                public void Get() { }
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().HaveCount( 3 ).And.OnlyContain( diagnostic => diagnostic.Id == AV0001 );
    }

    [Fact]
    public async Task analyzer_should_report_invalid_api_version_annotated_by_internal_attribute()
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
            public sealed class BackportedVersionedAttribute : Attribute
            {
                public BackportedVersionedAttribute( [StringSyntax( "ApiVersion" )] string version ) { }
            }
            """;

        var library = AnalyzerVerifier.EmitAssembly( "Backported", Backport );
        var source = """
            [BackportedVersioned( "1.x" )]
            public class Controller
            {
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source, library );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0001 );
    }

    [Fact]
    public async Task analyzer_should_not_report_unrelated_string_syntax()
    {
        // arrange
        var source = """
            using System;
            using System.Diagnostics.CodeAnalysis;

            [AttributeUsage( AttributeTargets.Class )]
            public sealed class RoutedAttribute : Attribute
            {
                public RoutedAttribute( [StringSyntax( "Route" )] string template ) { }
            }

            [Routed( "not/an/api/version" )]
            public class Controller
            {
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    private static string Versioned( string version ) =>
        $$"""
        using Asp.Versioning;

        [ApiVersion( {{AnalyzerVerifier.Literal( version )}} )]
        public class Controller
        {
        }
        """;

    private static string Declared( string code ) => Declarations + Environment.NewLine + code;
}