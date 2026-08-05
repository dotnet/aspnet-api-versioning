// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class MissingDocumentInfoAnalyzerTest
{
    private const string AV0025 = nameof( AV0025 );
    private const string Description = """[assembly: System.Reflection.AssemblyDescription( "An example API." )]""";

    [Fact]
    public async Task analyzer_should_report_a_document_without_a_description()
    {
        // arrange
        var source = Configured( "services.AddApiVersioning().AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0025 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_description_written_by_hand()
    {
        // arrange
        var source = Configured( "services.AddApiVersioning().AddOpenApi();", Description );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_description_generated_from_the_project()
    {
        // arrange
        // the project generates the attribute into a file of its own, which is part of the compilation
        var generated = $"""
            [assembly: System.Reflection.AssemblyCompany( "Contoso" )]
            [assembly: System.Reflection.AssemblyTitle( "Example" )]
            {Description}
            """;
        var source = Configured( "services.AddApiVersioning().AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source, generated );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_a_description_that_is_empty()
    {
        // arrange
        // an empty description is left out of the document the same way a missing one is
        var source = Configured(
            "services.AddApiVersioning().AddOpenApi();",
            """[assembly: System.Reflection.AssemblyDescription( "" )]""" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0025 );
    }

    [Fact]
    public async Task analyzer_should_report_when_only_a_title_is_present()
    {
        // arrange
        // the project supplies a title whether it was asked for or not, which says nothing about the
        // description
        var source = Configured(
            "services.AddApiVersioning().AddOpenApi();",
            """[assembly: System.Reflection.AssemblyTitle( "Example" )]""" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0025 );
    }

    [Fact]
    public async Task analyzer_should_not_report_without_openapi()
    {
        // arrange
        var source = Configured( "services.AddApiVersioning().AddApiExplorer();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_library()
    {
        // arrange
        // the document is described from the assembly the application was started from
        var source = """
            using Asp.Versioning;
            using Microsoft.Extensions.DependencyInjection;

            public static class ServiceDefaults
            {
                public static void ConfigureServices( IServiceCollection services ) =>
                    services.AddApiVersioning().AddOpenApi();
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().NotContain( diagnostic => diagnostic.Id == AV0025 );
    }

    [Fact]
    public async Task analyzer_should_report_each_call_site()
    {
        // arrange
        var source = Configured( """
            services.AddApiVersioning().AddOpenApi();
            services.AddApiVersioning().AddOpenApi();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().HaveCount( 2 ).And.OnlyContain( diagnostic => diagnostic.Id == AV0025 );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_openapi_call_site()
    {
        // arrange
        var source = Configured( "services.AddApiVersioning().AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "AddOpenApi" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Info );
        diagnostic.Descriptor.Category.Should().Be( "Documentation" );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( OutputKind.ConsoleApplication, sources ) )
            .Where( diagnostic => diagnostic.Id == AV0025 )];

    private static string Configured( string body, string attributes = "" ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.Extensions.DependencyInjection;

        {{attributes}}

        public static class Program
        {
            public static void Main()
            {
            }

            public static void ConfigureServices( IServiceCollection services )
            {
                {{body}}
            }
        }
        """;
}