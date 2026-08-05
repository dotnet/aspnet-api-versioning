// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class InheritedApiExplorerOptionAnalyzerTest
{
    private const string AV0024 = nameof( AV0024 );

    [Theory]
    [InlineData( "new ApiVersion( 2, 0 )" )]
    [InlineData( "new ApiVersion( 2.0 )" )]
    [InlineData( "new ApiVersion( 1, 1, \"beta\" )" )]
    public async Task analyzer_should_report_a_value_matching_api_versioning( string version )
    {
        // arrange
        var source = Configured(
            $"versioning.DefaultApiVersion = {version}",
            $"explorer.DefaultApiVersion = {version}" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0024 );
    }

    [Theory]
    [InlineData( "new ApiVersion( 2, 0 )", "new ApiVersion( 2.0 )" )]
    [InlineData( "new ApiVersion( 1 )", "new ApiVersion( 1, 0 )" )]
    [InlineData( "ApiVersion.Default", "new ApiVersion( 1.0 )" )]
    public async Task analyzer_should_report_the_same_version_written_two_ways(
        string configured,
        string restated )
    {
        // arrange
        var source = Configured(
            $"versioning.DefaultApiVersion = {configured}",
            $"explorer.DefaultApiVersion = {restated}" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0024 );
    }

    [Theory]
    [InlineData( "ApiVersion.Default" )]
    [InlineData( "new ApiVersion( 1, 0 )" )]
    [InlineData( "new ApiVersion( 1 )" )]
    public async Task analyzer_should_report_the_default_when_api_versioning_states_none( string version )
    {
        // arrange
        // the versioning options decide the default, and they default to 1.0 themselves
        var source = Configured( "versioning.ReportApiVersions = true", $"explorer.DefaultApiVersion = {version}" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0024 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_value_differing_from_api_versioning()
    {
        // arrange
        // the API explorer is meant to describe a different default than the one requests resolve to
        var source = Configured(
            "versioning.DefaultApiVersion = new ApiVersion( 2, 0 )",
            "explorer.DefaultApiVersion = new ApiVersion( 1, 0 )" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_value_other_than_the_default()
    {
        // arrange
        var source = Configured(
            "versioning.ReportApiVersions = true",
            "explorer.DefaultApiVersion = new ApiVersion( 2, 0 )" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "AssumeDefaultVersionWhenUnspecified", "true", "AssumeDefaultVersionWhenUnspecified", "true" )]
    [InlineData( "RouteConstraintName", "\"version\"", "RouteConstraintName", "\"version\"" )]
    [InlineData( "ApiVersionReader", "new QueryStringApiVersionReader()", "ApiVersionParameterSource", "new QueryStringApiVersionReader()" )]
    [InlineData( "ApiVersionReader", "new HeaderApiVersionReader( \"api-version\" )", "ApiVersionParameterSource", "new HeaderApiVersionReader( \"api-version\" )" )]
    public async Task analyzer_should_report_each_shared_property(
        string source,
        string configured,
        string target,
        string restated )
    {
        // arrange
        var code = Configured( $"versioning.{source} = {configured}", $"explorer.{target} = {restated}" );

        // act
        var diagnostics = await AnalyzeAsync( code );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0024 );
    }

    [Theory]
    [InlineData( "AssumeDefaultVersionWhenUnspecified", "false" )]
    [InlineData( "RouteConstraintName", "\"apiVersion\"" )]
    [InlineData( "ApiVersionParameterSource", "ApiVersionReader.Default" )]
    public async Task analyzer_should_report_a_shared_default_when_api_versioning_states_none(
        string property,
        string value )
    {
        // arrange
        var source = Configured( "versioning.ReportApiVersions = true", $"explorer.{property} = {value}" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0024 );
    }

    [Theory]
    [InlineData( "AssumeDefaultVersionWhenUnspecified", "true" )]
    [InlineData( "RouteConstraintName", "\"version\"" )]
    [InlineData( "ApiVersionParameterSource", "new HeaderApiVersionReader( \"api-version\" )" )]
    public async Task analyzer_should_not_report_a_shared_value_other_than_the_default(
        string property,
        string value )
    {
        // arrange
        var source = Configured( "versioning.ReportApiVersions = true", $"explorer.{property} = {value}" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_value_the_options_do_not_share()
    {
        // arrange
        // the group name format belongs to the API explorer alone
        var source = Configured( "versioning.ReportApiVersions = true", "explorer.GroupNameFormat = \"VVV\"" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_a_descendent_of_the_api_explorer_options()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Asp.Versioning.ApiExplorer;
            using Asp.Versioning.OData;

            public static class Startup
            {
                public static void Configure( ApiVersioningOptions versioning, ODataApiExplorerOptions explorer )
                {
                    versioning.DefaultApiVersion = new ApiVersion( 2, 0 );
                    explorer.DefaultApiVersion = new ApiVersion( 2, 0 );
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0024 );
    }

    [Fact]
    public async Task analyzer_should_report_across_files()
    {
        // arrange
        var versioning = """
            using Asp.Versioning;
            using Microsoft.Extensions.Options;

            public class ConfigureApiVersioning : IConfigureOptions<ApiVersioningOptions>
            {
                public void Configure( ApiVersioningOptions versioning ) =>
                    versioning.DefaultApiVersion = new ApiVersion( 2, 0 );
            }
            """;
        var explorer = """
            using Asp.Versioning;
            using Asp.Versioning.ApiExplorer;
            using Microsoft.Extensions.Options;

            public class ConfigureApiExplorer : IConfigureOptions<ApiExplorerOptions>
            {
                public void Configure( ApiExplorerOptions explorer ) =>
                    explorer.DefaultApiVersion = new ApiVersion( 2, 0 );
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( versioning, explorer );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0024 );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_api_versioning_is_configured_two_ways()
    {
        // arrange
        // nothing can be said about which value the API explorer is given
        var source = """
            using Asp.Versioning;
            using Asp.Versioning.ApiExplorer;

            public static class Startup
            {
                public static void Configure(
                    ApiVersioningOptions first,
                    ApiVersioningOptions second,
                    ApiExplorerOptions explorer )
                {
                    first.DefaultApiVersion = new ApiVersion( 2, 0 );
                    second.DefaultApiVersion = new ApiVersion( 3, 0 );
                    explorer.DefaultApiVersion = new ApiVersion( 2, 0 );
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_value_known_only_at_run_time()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Asp.Versioning.ApiExplorer;

            public static class Startup
            {
                public static void Configure( ApiVersioningOptions versioning, ApiExplorerOptions explorer, ApiVersion version )
                {
                    versioning.DefaultApiVersion = version;
                    explorer.DefaultApiVersion = version;
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_across_the_assignment_as_unnecessary_code()
    {
        // arrange
        var source = Configured(
            "versioning.DefaultApiVersion = new ApiVersion( 2, 0 )",
            "explorer.DefaultApiVersion = new ApiVersion( 2, 0 )" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "explorer.DefaultApiVersion = new ApiVersion( 2, 0 )" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Info );
        diagnostic.Descriptor.CustomTags.Should().Contain( WellKnownDiagnosticTags.Unnecessary );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( sources ) ).Where( diagnostic => diagnostic.Id == AV0024 )];

    private static string Configured( string versioning, string explorer ) =>
        $$"""
        using Asp.Versioning;
        using Asp.Versioning.ApiExplorer;

        public static class Startup
        {
            public static void Configure( ApiVersioningOptions versioning, ApiExplorerOptions explorer )
            {
                {{versioning}};
                {{explorer}};
            }
        }
        """;
}