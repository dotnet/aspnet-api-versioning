// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class MissingApiExplorerAnalyzerTest
{
    private const string AV0031 = nameof( AV0031 );

    [Theory]
    [InlineData( "services.AddApiVersioning().AddOpenApi();" )]
    [InlineData( "services.AddApiVersioning().AddMvc().AddOpenApi();" )]
    public async Task analyzer_should_report_the_missing_api_explorer( string chain )
    {
        // arrange
        var source = Configured( chain );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;

        diagnostic.Id.Should().Be( AV0031 );
        diagnostic.GetMessage().Should().Contain( "AddApiExplorer()" );
    }

    [Fact]
    public async Task analyzer_should_report_the_missing_odata_api_explorer()
    {
        // arrange
        var source = Configured( "services.AddApiVersioning().AddOData().AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;

        diagnostic.Id.Should().Be( AV0031 );
        diagnostic.GetMessage().Should().Contain( "AddODataApiExplorer()" );
    }

    [Fact]
    public async Task analyzer_should_report_the_missing_grpc_api_explorer()
    {
        // arrange
        var source = Configured( "services.AddApiVersioning().AddGrpc().AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;

        diagnostic.Id.Should().Be( AV0031 );
        diagnostic.GetMessage().Should().Contain( "AddGrpcApiExplorer()" );
    }

    [Fact]
    public async Task analyzer_should_report_each_missing_api_explorer()
    {
        // arrange
        // an application can be built more than one way, and each way is described on its own
        var source = Configured( "services.AddApiVersioning().AddOData().AddGrpc().AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Select( diagnostic => diagnostic.GetMessage() )
                   .Should()
                   .HaveCount( 2 )
                   .And.Contain( message => message.Contains( "AddODataApiExplorer()" ) )
                   .And.Contain( message => message.Contains( "AddGrpcApiExplorer()" ) );
    }

    [Theory]
    [InlineData( "services.AddApiVersioning().AddApiExplorer().AddOpenApi();" )]
    [InlineData( "services.AddApiVersioning().AddMvc().AddApiExplorer().AddOpenApi();" )]
    [InlineData( "services.AddApiVersioning().AddOData().AddODataApiExplorer().AddOpenApi();" )]
    [InlineData( "services.AddApiVersioning().AddGrpc().AddGrpcApiExplorer().AddOpenApi();" )]
    public async Task analyzer_should_not_report_a_configured_api_explorer( string chain )
    {
        // arrange
        var source = Configured( chain );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_the_base_explorer_alongside_a_specialized_one()
    {
        // arrange
        // OData is described by an explorer that builds on the one the rest of them use
        var source = Configured( "services.AddApiVersioning().AddOData().AddODataApiExplorer().AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "services.AddApiVersioning().AddGrpcApiExplorer().AddOpenApi();" )]
    [InlineData( "services.AddApiVersioning().AddODataApiExplorer().AddOpenApi();" )]
    public async Task analyzer_should_not_report_a_specialized_explorer_used_on_its_own( string chain )
    {
        // arrange
        // a specialized explorer can be configured without the APIs it specializes in
        var source = Configured( chain );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_grpc_without_its_explorer_however_it_is_described()
    {
        // arrange
        // the base explorer says nothing about gRPC, which is described by an explorer of its own
        var source = Configured( "services.AddApiVersioning().AddGrpc().AddApiExplorer().AddOpenApi();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "AddGrpcApiExplorer()" );
    }

    [Fact]
    public async Task analyzer_should_not_report_without_openapi()
    {
        // arrange
        // nothing is generating a document for the explorer to describe
        var source = Configured( "services.AddApiVersioning().AddOData();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_the_services_of_the_same_name()
    {
        // arrange
        // gRPC and MVC declare methods of their own that say nothing about API versioning
        var source = """
            using Asp.Versioning;
            using Microsoft.Extensions.DependencyInjection;

            public static class Startup
            {
                public static void Configure( IServiceCollection services )
                {
                    services.AddGrpc();
                    services.AddMvc();
                    services.AddApiVersioning().AddApiExplorer().AddOpenApi();
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_configuration_split_across_variables()
    {
        // arrange
        // which builder the calls were made against is not tracked
        var source = """
            using Asp.Versioning;
            using Microsoft.Extensions.DependencyInjection;

            public static class Startup
            {
                public static void Configure( IServiceCollection services )
                {
                    var builder = services.AddApiVersioning();

                    builder.AddOData();
                    builder.AddOpenApi();
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "AddODataApiExplorer()" );
    }

    [Fact]
    public async Task analyzer_should_not_report_configuration_split_across_methods()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Microsoft.Extensions.DependencyInjection;

            public static class Startup
            {
                public static void Configure( IServiceCollection services ) =>
                    Describe( services.AddApiVersioning().AddOData() ).AddOpenApi();

                private static IApiVersioningBuilder Describe( IApiVersioningBuilder builder ) =>
                    builder.AddODataApiExplorer();
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
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
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Warning );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( sources ) ).Where( diagnostic => diagnostic.Id == AV0031 )];

    private static string Configured( string chain ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.Extensions.DependencyInjection;

        public static class Startup
        {
            public static void Configure( IServiceCollection services )
            {
                {{chain}}
            }
        }
        """;
}