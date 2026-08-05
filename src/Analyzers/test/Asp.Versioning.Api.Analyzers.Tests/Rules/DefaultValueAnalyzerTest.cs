// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class DefaultValueAnalyzerTest
{
    private const string AV0017 = nameof( AV0017 );

    [Theory]
    [InlineData( "options.RouteConstraintName = \"apiVersion\"" )]
    [InlineData( "options.ReportApiVersions = false" )]
    [InlineData( "options.AssumeDefaultVersionWhenUnspecified = false" )]
    [InlineData( "options.UnsupportedApiVersionStatusCode = 400" )]
    public async Task analyzer_should_report_a_default_on_api_versioning_options( string assignment )
    {
        // arrange
        var source = Configured( "ApiVersioningOptions", assignment );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0017 );
    }

    [Theory]
    [InlineData( "options.RouteConstraintName = \"version\"" )]
    [InlineData( "options.ReportApiVersions = true" )]
    [InlineData( "options.AssumeDefaultVersionWhenUnspecified = true" )]
    [InlineData( "options.UnsupportedApiVersionStatusCode = 404" )]
    public async Task analyzer_should_not_report_a_value_other_than_the_default( string assignment )
    {
        // arrange
        var source = Configured( "ApiVersioningOptions", assignment );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "options.GroupNameFormat = \"\"" )]
    [InlineData( "options.GroupNameFormat = string.Empty" )]
    [InlineData( "options.SubstitutionFormat = \"VVV\"" )]
    [InlineData( "options.SubstituteApiVersionInUrl = false" )]
    [InlineData( "options.AddApiVersionParametersWhenVersionNeutral = false" )]
    [InlineData( "options.FormatGroupName = null" )]
    public async Task analyzer_should_report_a_default_on_api_explorer_options( string assignment )
    {
        // arrange
        var source = Configured( "ApiExplorerOptions", assignment );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0017 );
    }

    [Theory]
    [InlineData( "options.UseQualifiedNames = false" )]
    [InlineData( "options.MetadataOptions = ODataMetadataOptions.None" )]
    [InlineData( "options.SubstituteApiVersionInUrl = false" )]
    public async Task analyzer_should_report_a_default_on_a_descendent( string assignment )
    {
        // arrange
        var source = Configured( "ODataApiExplorerOptions", assignment );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0017 );
    }

    [Theory]
    [InlineData( "options.AssumeDefaultVersionWhenUnspecified = false" )]
    [InlineData( "options.RouteConstraintName = string.Empty" )]
    [InlineData( "options.RouteConstraintName = \"apiVersion\"" )]
    [InlineData( "options.DefaultApiVersion = ApiVersion.Default" )]
    public async Task analyzer_should_not_report_a_value_shared_with_api_versioning( string assignment )
    {
        // arrange
        // what the API explorer defaults to is decided by the versioning options rather than by the
        // property, so a shared value is reported on its own
        var source = Configured( "ApiExplorerOptions", assignment );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_the_default_api_version()
    {
        // arrange
        // the default version can be spelled several ways and is reported on its own
        var source = Configured( "ApiVersioningOptions", "options.DefaultApiVersion = ApiVersion.Default" );

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

            public static class Startup
            {
                public static void Configure( ApiVersioningOptions options, bool report ) =>
                    options.ReportApiVersions = report;
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_an_unrelated_option()
    {
        // arrange
        var source = """
            public class Unrelated
            {
                public bool ReportApiVersions { get; set; }
            }

            public static class Startup
            {
                public static void Configure( Unrelated options ) => options.ReportApiVersions = false;
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
        var source = Configured( "ApiVersioningOptions", "options.ReportApiVersions = false" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "options.ReportApiVersions = false" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Info );
        diagnostic.Descriptor.CustomTags.Should().Contain( WellKnownDiagnosticTags.Unnecessary );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( string source ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( source ) ).Where( diagnostic => diagnostic.Id == AV0017 )];

    private static string Configured( string options, string assignment ) =>
        $$"""
        using System;
        using Asp.Versioning;
        using Asp.Versioning.ApiExplorer;
        using Asp.Versioning.OData;

        public static class Startup
        {
            public static void Configure( {{options}} options ) => {{assignment}};
        }
        """;
}