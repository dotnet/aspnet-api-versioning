// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class DescribeApiVersionsAnalyzerTest
{
    private const string AV0027 = nameof( AV0027 );

    [Theory]
    [InlineData( "AddApiExplorer" )]
    [InlineData( "AddODataApiExplorer" )]
    [InlineData( "AddGrpcApiExplorer" )]
    [InlineData( "AddOpenApi" )]
    public async Task analyzer_should_report_for_each_api_explorer( string explorer )
    {
        // arrange
        var source = Configured(
            $"builder.Services.AddApiVersioning().{explorer}();",
            """app.MapGet( "/order", () => "" );""",
            "app.Services.GetRequiredService<IApiVersionDescriptionProvider>()" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0027 );
    }

    [Theory]
    [InlineData( "app.Services.GetService<IApiVersionDescriptionProvider>()" )]
    [InlineData( "app.Services.GetRequiredService<IApiVersionDescriptionProvider>()" )]
    [InlineData( "app.Services.GetService( typeof( IApiVersionDescriptionProvider ) )" )]
    [InlineData( "app.Services.GetRequiredService( typeof( IApiVersionDescriptionProvider ) )" )]
    public async Task analyzer_should_report_each_way_the_provider_is_resolved( string resolution )
    {
        // arrange
        var source = Configured(
            "builder.Services.AddApiVersioning().AddApiExplorer();",
            """app.MapGet( "/order", () => "" );""",
            resolution );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0027 );
    }

    [Fact]
    public async Task analyzer_should_not_report_without_a_minimal_api()
    {
        // arrange
        // the services knew about every API there was by the time they were built
        var source = Configured(
            "builder.Services.AddApiVersioning().AddApiExplorer();",
            "",
            "app.Services.GetRequiredService<IApiVersionDescriptionProvider>()" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_without_an_api_explorer()
    {
        // arrange
        var source = Configured(
            "builder.Services.AddApiVersioning();",
            """app.MapGet( "/order", () => "" );""",
            "app.Services.GetRequiredService<IApiVersionDescriptionProvider>()" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_another_service()
    {
        // arrange
        var source = Configured(
            "builder.Services.AddApiVersioning().AddApiExplorer();",
            """app.MapGet( "/order", () => "" );""",
            "app.Services.GetRequiredService<IApiVersionParser>()" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_the_endpoints_that_describe_themselves()
    {
        // arrange
        // describing from the application waits until every API has been mapped
        var source = """
            using Asp.Versioning;
            using Asp.Versioning.ApiExplorer;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.DependencyInjection;

            public static class Program
            {
                public static void Main()
                {
                    var builder = WebApplication.CreateBuilder();

                    builder.Services.AddApiVersioning().AddApiExplorer();

                    var app = builder.Build();

                    app.MapGet( "/order", () => "" );

                    var descriptions = app.DescribeApiVersions();
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_a_resolution_inside_a_callback()
    {
        // arrange
        // the UI is configured by a callback that closes over the application, which is where the
        // descriptions are reached from
        var source = """
            using System;
            using Asp.Versioning;
            using Asp.Versioning.ApiExplorer;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.DependencyInjection;

            public static class Program
            {
                public static void Main()
                {
                    var builder = WebApplication.CreateBuilder();

                    builder.Services.AddApiVersioning().AddApiExplorer();

                    var app = builder.Build();

                    app.MapGet( "/order", () => "" );

                    Action configure = () =>
                    {
                        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

                        foreach ( var description in provider.ApiVersionDescriptions )
                        {
                        }
                    };
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0027 );
    }

    [Fact]
    public async Task analyzer_should_not_report_descriptions_taken_inside_a_callback()
    {
        // arrange
        // the same closure describing the versions from the application instead
        var source = """
            using System;
            using Asp.Versioning;
            using Asp.Versioning.ApiExplorer;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.Extensions.DependencyInjection;

            public static class Program
            {
                public static void Main()
                {
                    var builder = WebApplication.CreateBuilder();

                    builder.Services.AddApiVersioning().AddApiExplorer();

                    var app = builder.Build();

                    app.MapGet( "/order", () => "" );

                    Action configure = () =>
                    {
                        foreach ( var description in app.DescribeApiVersions() )
                        {
                        }
                    };
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_each_resolution()
    {
        // arrange
        const string Twice = "app.Services.GetRequiredService<IApiVersionDescriptionProvider>();" +
                             "var second = app.Services.GetRequiredService<IApiVersionDescriptionProvider>()";
        var source = Configured(
            "builder.Services.AddApiVersioning().AddApiExplorer();",
            """app.MapGet( "/order", () => "" );""",
            Twice );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().HaveCount( 2 ).And.OnlyContain( diagnostic => diagnostic.Id == AV0027 );
    }

    [Fact]
    public async Task analyzer_should_report_across_files()
    {
        // arrange
        var api = """
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Routing;

            public static class Orders
            {
                public static void MapOrders( this IEndpointRouteBuilder endpoints ) =>
                    endpoints.MapGet( "/order", () => "" );
            }
            """;
        var startup = Configured(
            "builder.Services.AddApiVersioning().AddApiExplorer();",
            "",
            "app.Services.GetRequiredService<IApiVersionDescriptionProvider>()" );

        // act
        var diagnostics = await AnalyzeAsync( startup, api );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0027 );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_resolution_call_site()
    {
        // arrange
        var source = Configured(
            "builder.Services.AddApiVersioning().AddApiExplorer();",
            """app.MapGet( "/order", () => "" );""",
            "app.Services.GetRequiredService<IApiVersionDescriptionProvider>()" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "GetRequiredService<IApiVersionDescriptionProvider>" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Warning );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( sources ) ).Where( diagnostic => diagnostic.Id == AV0027 )];

    private static string Configured( string services, string endpoints, string resolution ) =>
        $$"""
        using Asp.Versioning;
        using Asp.Versioning.ApiExplorer;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.Extensions.DependencyInjection;

        public static class Program
        {
            public static void Main()
            {
                var builder = WebApplication.CreateBuilder();

                {{services}}

                var app = builder.Build();

                {{endpoints}}

                var provider = {{resolution}};
            }
        }
        """;
}