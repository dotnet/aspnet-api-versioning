// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class VersionedOpenApiAnalyzerTest
{
    private const string AV0029 = nameof( AV0029 );
    private const string AV0030 = nameof( AV0030 );

    [Theory]
    [InlineData( "AddApiExplorer" )]
    [InlineData( "AddODataApiExplorer" )]
    [InlineData( "AddGrpcApiExplorer" )]
    [InlineData( "AddOpenApi" )]
    public async Task analyzer_should_report_openapi_services_for_each_explorer( string explorer )
    {
        // arrange
        var source = Configured( $"services.AddApiVersioning().{explorer}();", "services.AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0029 );
    }

    [Theory]
    [InlineData( "services.AddOpenApi();" )]
    [InlineData( """services.AddOpenApi( "v1" );""" )]
    [InlineData( "services.AddOpenApi( options => { } );" )]
    public async Task analyzer_should_report_any_form_of_openapi_services( string added )
    {
        // arrange
        var source = Configured( "services.AddApiVersioning().AddOpenApi();", added );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0029 );
    }

    [Fact]
    public async Task analyzer_should_not_report_openapi_services_without_versioning()
    {
        // arrange
        // an application that does not version its APIs is described by a single document
        var source = Configured( "services.AddApiVersioning();", "services.AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_the_services_as_unnecessary_code()
    {
        // arrange
        var source = Configured( "services.AddApiVersioning().AddOpenApi();", "services.AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "services.AddOpenApi();" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Warning );
        diagnostic.Descriptor.CustomTags.Should().Contain( WellKnownDiagnosticTags.Unnecessary );
    }

    [Fact]
    public async Task analyzer_should_report_a_document_that_is_not_per_version()
    {
        // arrange
        var source = Mapped( "app.MapOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0030 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_document_per_version()
    {
        // arrange
        var source = Mapped( "app.MapOpenApi().WithDocumentPerVersion();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_document_per_version_among_other_conventions()
    {
        // arrange
        var source = Mapped( """app.MapOpenApi().WithGroupName( "docs" ).WithDocumentPerVersion();""" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_convention_applied_some_other_way()
    {
        // arrange
        // the convention may well belong to the mapped endpoint, which cannot be told from here
        var source = Mapped( """
            var openApi = app.MapOpenApi();
            openApi.WithDocumentPerVersion();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_mapped_document_without_versioning()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.DependencyInjection;

            public static class Startup
            {
                public static void Configure( WebApplication app, IServiceCollection services )
                {
                    services.AddApiVersioning();
                    app.MapOpenApi();
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_both_rules_together()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.DependencyInjection;

            public static class Startup
            {
                public static void Configure( WebApplication app, IServiceCollection services )
                {
                    services.AddApiVersioning().AddOpenApi();
                    services.AddOpenApi();
                    app.MapOpenApi();
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Select( diagnostic => diagnostic.Id ).Should().BeEquivalentTo( [AV0029, AV0030] );
    }

    [Fact]
    public async Task analyzer_should_report_each_call_site()
    {
        // arrange
        var source = Mapped( """
            app.MapOpenApi();
            app.MapOpenApi();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().HaveCount( 2 ).And.OnlyContain( diagnostic => diagnostic.Id == AV0030 );
    }

    [Fact]
    public async Task analyzer_should_report_across_files()
    {
        // arrange
        var services = Configured( "services.AddApiVersioning().AddOpenApi();", "", "Services" );
        var endpoints = """
            using Microsoft.AspNetCore.Builder;

            public static class Endpoints
            {
                public static void Configure( WebApplication app ) => app.MapOpenApi();
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( services, endpoints );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0030 );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_mapped_call_site()
    {
        // arrange
        var source = Mapped( "app.MapOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "MapOpenApi" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Warning );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( sources ) )
            .Where( diagnostic => diagnostic.Id == AV0029 || diagnostic.Id == AV0030 )];

    private static string Configured( string versioning, string openApi, string name = "Startup" ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;

        public static class {{name}}
        {
            public static void Configure( IServiceCollection services )
            {
                {{versioning}}
                {{openApi}}
            }
        }
        """;

    private static string Mapped( string endpoints ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;

        public static class Startup
        {
            public static void Configure( WebApplication app, IServiceCollection services )
            {
                services.AddApiVersioning().AddOpenApi();
                {{endpoints}}
            }
        }
        """;
}