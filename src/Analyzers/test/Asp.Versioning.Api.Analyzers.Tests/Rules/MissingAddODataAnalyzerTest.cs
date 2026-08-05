// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class MissingAddODataAnalyzerTest
{
    private const string AV0022 = nameof( AV0022 );

    [Fact]
    public async Task analyzer_should_report_unversioned_odata()
    {
        // arrange
        var source = Configured( """
            services.AddControllers().AddOData();
            services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0022 );
    }

    [Fact]
    public async Task analyzer_should_report_unversioned_odata_from_mvc_core()
    {
        // arrange
        var source = Configured( """
            services.AddMvcCore().AddOData();
            services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0022 );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_api_versioning_call_site()
    {
        // arrange
        var source = Configured( """
            services.AddControllers().AddOData();
            services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "AddApiVersioning" );
    }

    [Fact]
    public async Task analyzer_should_not_report_versioned_odata()
    {
        // arrange
        var source = Configured( """
            services.AddControllers().AddOData();
            services.AddApiVersioning().AddOData();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_the_odata_api_explorer_alone()
    {
        // arrange
        // the explorer registers the versioned services it needs without the rest of them
        var source = Configured( """
            services.AddControllers().AddOData();
            services.AddApiVersioning().AddODataApiExplorer();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_without_odata()
    {
        // arrange
        var source = Configured( """
            services.AddControllers();
            services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_without_api_versioning()
    {
        // arrange
        var source = Configured( "services.AddControllers().AddOData();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_across_files()
    {
        // arrange
        var odata = Configured( "services.AddControllers().AddOData();", "Data" );
        var versioning = Configured( "services.AddApiVersioning();", "Versioning" );

        // act
        var diagnostics = await AnalyzeAsync( odata, versioning );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0022 );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_versioned_odata_is_in_another_file()
    {
        // arrange
        var odata = Configured( "services.AddControllers().AddOData();", "Data" );
        var versioning = Configured( "services.AddApiVersioning().AddOData();", "Versioning" );

        // act
        var diagnostics = await AnalyzeAsync( odata, versioning );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_each_api_versioning_call_site()
    {
        // arrange
        var source = Configured( """
            services.AddControllers().AddOData();
            services.AddApiVersioning();
            services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().HaveCount( 2 ).And.OnlyContain( diagnostic => diagnostic.Id == AV0022 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_user_defined_add_odata()
    {
        // arrange
        var source = """
            using Microsoft.Extensions.DependencyInjection;

            public static class ODataExtensions
            {
                public static IServiceCollection AddOData( this IServiceCollection services ) => services;
            }

            public static class Startup
            {
                public static void ConfigureServices( IServiceCollection services )
                {
                    services.AddOData();
                    services.AddApiVersioning();
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( sources ) )
            .Where( diagnostic => diagnostic.Id == AV0022 )];

    private static string Configured( string body, string name = "Startup" ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.AspNetCore.OData;
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