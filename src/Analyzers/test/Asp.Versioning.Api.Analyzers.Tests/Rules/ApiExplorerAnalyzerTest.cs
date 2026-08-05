// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class ApiExplorerAnalyzerTest
{
    private const string AV0020 = nameof( AV0020 );
    private const string AV0021 = nameof( AV0021 );

    [Fact]
    public async Task analyzer_should_report_a_redundant_endpoints_api_explorer()
    {
        // arrange
        // the versioned explorer adds the endpoints explorer itself
        var source = Configured( """
            services.AddEndpointsApiExplorer();
            services.AddApiVersioning().AddApiExplorer();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0020 );
    }

    [Fact]
    public async Task analyzer_should_report_an_unversioned_endpoints_api_explorer()
    {
        // arrange
        // versions are in use, but the explorer describing the endpoints knows nothing about them
        var source = Configured( """
            services.AddEndpointsApiExplorer();
            services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0021 );
    }

    [Fact]
    public async Task analyzer_should_not_report_the_versioned_api_explorer_alone()
    {
        // arrange
        var source = Configured( "services.AddApiVersioning().AddApiExplorer();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_a_redundant_endpoints_api_explorer_with_odata()
    {
        // arrange
        // the OData explorer reaches the versioned explorer, which adds the endpoints explorer itself
        var source = Configured( """
            services.AddEndpointsApiExplorer();
            services.AddApiVersioning().AddODataApiExplorer();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0020 );
    }

    [Fact]
    public async Task analyzer_should_report_a_redundant_endpoints_api_explorer_with_openapi()
    {
        // arrange
        // AddOpenApi reaches the versioned explorer the same way the OData explorer does
        var source = Configured( """
            services.AddEndpointsApiExplorer();
            services.AddApiVersioning().AddOpenApi();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0020 );
    }

    [Fact]
    public async Task analyzer_should_not_report_without_api_versioning()
    {
        // arrange
        // an application that does not version has no reason to use the versioned explorer
        var source = Configured( "services.AddEndpointsApiExplorer();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_each_endpoints_api_explorer()
    {
        // arrange
        var source = Configured( """
            services.AddEndpointsApiExplorer();
            services.AddEndpointsApiExplorer();
            services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().HaveCount( 2 ).And.OnlyContain( diagnostic => diagnostic.Id == AV0021 );
    }

    [Fact]
    public async Task analyzer_should_report_across_files()
    {
        // arrange
        var explorer = Configured( "services.AddEndpointsApiExplorer();", "Explorer" );
        var versioning = Configured( "services.AddApiVersioning().AddApiExplorer();", "Versioning" );

        // act
        var diagnostics = await AnalyzeAsync( explorer, versioning );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0020 );
    }

    [Fact]
    public async Task analyzer_should_report_the_redundant_call_as_unnecessary_code()
    {
        // arrange
        var source = Configured( """
            services.AddEndpointsApiExplorer();
            services.AddApiVersioning().AddApiExplorer();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "services.AddEndpointsApiExplorer();" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Info );
        diagnostic.Descriptor.CustomTags.Should().Contain( WellKnownDiagnosticTags.Unnecessary );
    }

    [Fact]
    public async Task analyzer_should_report_the_unversioned_call_as_a_warning()
    {
        // arrange
        var source = Configured( """
            services.AddEndpointsApiExplorer();
            services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Severity.Should().Be( DiagnosticSeverity.Warning );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( sources ) )
            .Where( diagnostic => diagnostic.Id is AV0020 or AV0021 )];

    private static string Configured( string body, string name = "Startup" ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.Extensions.DependencyInjection;

        public static class {{name}}
        {
            public static void ConfigureServices( IServiceCollection services )
            {
                {{body}}
            }
        }
        """;
}