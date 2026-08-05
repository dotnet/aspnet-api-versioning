// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class DefaultApiVersionAnalyzerTest
{
    private const string AV0011 = nameof( AV0011 );
    private const string AV0012 = nameof( AV0012 );

    [Theory]
    [InlineData( "ApiVersion.Default" )]
    [InlineData( "new ApiVersion( 1, 0 )" )]
    [InlineData( "new ApiVersion( 1 )" )]
    [InlineData( "new( 1, 0 )" )]
    [InlineData( "new ApiVersion( 1.0 )" )]
    [InlineData( "new ApiVersion( majorVersion: 1, minorVersion: 0 )" )]
    [InlineData( "new ApiVersion( minorVersion: 0, majorVersion: 1 )" )]
    public async Task analyzer_should_report_unnecessary_default_api_version( string version )
    {
        // arrange
        var source = Configured( "ApiVersioningOptions", version );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0011 );
    }

    [Fact]
    public async Task analyzer_should_report_neutral_default_api_version()
    {
        // arrange
        var source = Configured( "ApiVersioningOptions", "ApiVersion.Neutral" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0012 );
    }

    [Fact]
    public async Task analyzer_should_report_the_options_that_decide_the_default()
    {
        // arrange
        var source = Configured( "ApiVersioningOptions", "ApiVersion.Default" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0011 );
    }

    [Fact]
    public async Task analyzer_should_not_report_the_api_explorer_default()
    {
        // arrange
        // the API explorer is given whatever default the versioning options were given, so a version
        // that matches is reported against what it came from rather than against the version here
        var source = Configured( "ApiExplorerOptions", "ApiVersion.Default" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().NotContain( diagnostic => diagnostic.Id == AV0011 );
    }

    [Theory]
    [InlineData( "ApiVersion.Neutral", AV0012 )]
    public async Task analyzer_should_report_for_a_descendent_of_options( string version, string expected )
    {
        // arrange
        var source = $$"""
            using Asp.Versioning;
            using Asp.Versioning.ApiExplorer;

            public class CustomApiExplorerOptions : ApiExplorerOptions
            {
            }

            public class Startup
            {
                public void Configure( CustomApiExplorerOptions options ) =>
                    options.DefaultApiVersion = {{version}};
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( expected );
    }

    [Theory]
    [InlineData( "ApiVersion.Default", AV0011 )]
    [InlineData( "ApiVersion.Neutral", AV0012 )]
    public async Task analyzer_should_report_in_an_object_initializer( string version, string expected )
    {
        // arrange
        var source = $$"""
            using Asp.Versioning;

            public class Startup
            {
                public ApiVersioningOptions Configure() =>
                    new() { DefaultApiVersion = {{version}} };
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( expected );
    }

    [Theory]
    [InlineData( "ApiVersion.Default", AV0011 )]
    [InlineData( "ApiVersion.Neutral", AV0012 )]
    public async Task analyzer_should_report_when_configured_by_options_setup( string version, string expected )
    {
        // arrange
        // the rule matches the assignment, so where the options are configured does not matter
        var source = $$"""
            using Asp.Versioning;
            using Microsoft.Extensions.Options;

            public class ConfigureApiVersioning : IConfigureOptions<ApiVersioningOptions>
            {
                public void Configure( ApiVersioningOptions options ) =>
                    options.DefaultApiVersion = {{version}};
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( expected );
    }

    [Theory]
    [InlineData( "ApiVersion.Neutral", AV0012 )]
    public async Task analyzer_should_report_when_configured_after_setup( string version, string expected )
    {
        // arrange
        var source = $$"""
            using Asp.Versioning;
            using Asp.Versioning.ApiExplorer;
            using Microsoft.Extensions.Options;

            public class PostConfigureApiExplorer : IPostConfigureOptions<ApiExplorerOptions>
            {
                public void PostConfigure( string name, ApiExplorerOptions options ) =>
                    options.DefaultApiVersion = {{version}};
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( expected );
    }

    [Theory]
    [InlineData( "new ApiVersion( 2, 0 )" )]
    [InlineData( "new ApiVersion( 1, 1 )" )]
    [InlineData( "new ApiVersion( 0, 9 )" )]
    [InlineData( "new ApiVersion( 2.0 )" )]
    [InlineData( "new ApiVersion( 1, 0, \"beta\" )" )]
    [InlineData( "new ApiVersion( 1.0, \"beta\" )" )]
    [InlineData( "new ApiVersion( new DateOnly( 2016, 1, 1 ) )" )]
    public async Task analyzer_should_not_report_a_version_other_than_the_default( string version )
    {
        // arrange
        var source = Configured( "ApiVersioningOptions", version );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_version_known_only_at_run_time()
    {
        // arrange
        var source = """
            using Asp.Versioning;

            public class Startup
            {
                public void Configure( ApiVersioningOptions options, ApiVersion version ) =>
                    options.DefaultApiVersion = version;
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_another_property()
    {
        // arrange
        // the rules are about the default version, not any other version an option may hold
        var source = """
            using Asp.Versioning;

            public class Startup
            {
                public void Configure( ApiVersioningOptions options ) =>
                    options.AssumeDefaultVersionWhenUnspecified = true;
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_an_unrelated_default_api_version()
    {
        // arrange
        // the property name matches, but the type it is declared on is not a versioning option
        var source = """
            using Asp.Versioning;

            public class Unrelated
            {
                public ApiVersion DefaultApiVersion { get; set; }
            }

            public class Startup
            {
                public void Configure( Unrelated options ) =>
                    options.DefaultApiVersion = ApiVersion.Default;
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_unnecessary_default_api_version_as_style()
    {
        // arrange
        var source = Configured( "ApiVersioningOptions", "ApiVersion.Default" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;

        diagnostic.Severity.Should().Be( DiagnosticSeverity.Info );
        diagnostic.Descriptor.Category.Should().Be( "Style" );
    }

    [Fact]
    public async Task analyzer_should_report_neutral_default_api_version_as_usage()
    {
        // arrange
        var source = Configured( "ApiVersioningOptions", "ApiVersion.Neutral" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;

        diagnostic.Severity.Should().Be( DiagnosticSeverity.Error );
        diagnostic.Descriptor.Category.Should().Be( "Usage" );
    }

    [Fact]
    public async Task analyzer_should_report_unnecessary_default_api_version_as_unnecessary_code()
    {
        // arrange
        // the tag is what fades the code out in an IDE rather than marking it as a problem
        var source = Configured( "ApiVersioningOptions", "ApiVersion.Default" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should()
                   .ContainSingle()
                   .Which.Descriptor.CustomTags
                   .Should()
                   .Contain( WellKnownDiagnosticTags.Unnecessary );
    }

    [Fact]
    public async Task analyzer_should_report_unnecessary_default_api_version_across_the_assignment()
    {
        // arrange
        // the entire assignment can be removed, so that is what is faded out
        var source = Configured( "ApiVersioningOptions", "ApiVersion.Default" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length )
              .Should()
              .Be( "options.DefaultApiVersion = ApiVersion.Default" );
    }

    [Fact]
    public async Task analyzer_should_not_report_neutral_default_api_version_as_unnecessary_code()
    {
        // arrange
        // a neutral version is wrong rather than redundant, so it must not be faded out
        var source = Configured( "ApiVersioningOptions", "ApiVersion.Neutral" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should()
                   .ContainSingle()
                   .Which.Descriptor.CustomTags
                   .Should()
                   .NotContain( WellKnownDiagnosticTags.Unnecessary );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_assigned_version()
    {
        // arrange
        var source = Configured( "ApiVersioningOptions", "ApiVersion.Neutral" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "ApiVersion.Neutral" );
    }

    private static string Configured( string options, string version ) =>
        $$"""
        using System;
        using Asp.Versioning;
        using Asp.Versioning.ApiExplorer;

        public class Startup
        {
            public void Configure( {{options}} options ) =>
                options.DefaultApiVersion = {{version}};
        }
        """;
}