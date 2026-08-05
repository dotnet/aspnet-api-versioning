// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class MissingAddMvcAnalyzerTest
{
    private const string AV0013 = nameof( AV0013 );

    [Theory]
    [InlineData( "services.AddControllers();" )]
    [InlineData( "services.AddControllers( options => { } );" )]
    [InlineData( "services.AddMvcCore();" )]
    [InlineData( "services.AddMvcCore( options => { } );" )]
    public async Task analyzer_should_report_controllers_without_versioned_mvc( string controllers )
    {
        // arrange
        var source = Startup( controllers + "\n        services.AddApiVersioning();" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0013 );
    }

    [Theory]
    [InlineData( "services.AddApiVersioning().AddMvc();" )]
    [InlineData( "services.AddApiVersioning().AddMvc( options => { } );" )]
    [InlineData( "services.AddApiVersioning( options => { } ).AddMvc();" )]
    public async Task analyzer_should_not_report_when_mvc_is_versioned( string versioning )
    {
        // arrange
        var source = Startup( "services.AddControllers();\n        " + versioning );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_when_mvc_is_versioned_separately()
    {
        // arrange
        // the builder is often held rather than chained
        var source = Startup( """
            services.AddControllers();

                    var builder = services.AddApiVersioning();

                    builder.AddMvc();
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_without_controllers()
    {
        // arrange
        // a minimal API is versioned without ever adding MVC
        var source = Startup( "services.AddApiVersioning();" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_without_api_versioning()
    {
        // arrange
        // there is no call site to report against, and nothing was versioned to begin with
        var source = Startup( "services.AddControllers();" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_when_mvc_adds_its_own_unrelated_mvc()
    {
        // arrange
        // MVC declares an AddMvc of its own, which does not version anything
        var source = Startup( """
            services.AddControllers();
                    services.AddMvc();
                    services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0013 );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_mvc_is_versioned_in_another_file()
    {
        // arrange
        // the calls are compilation wide, so they need not appear together
        var controllers = Startup( "services.AddControllers();", "Controllers" );
        var versioning = Startup( "services.AddApiVersioning().AddMvc();", "Versioning" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( controllers, versioning );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_when_controllers_are_added_in_another_file()
    {
        // arrange
        var controllers = Startup( "services.AddControllers();", "Controllers" );
        var versioning = Startup( "services.AddApiVersioning();", "Versioning" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( controllers, versioning );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0013 );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_api_versioning_call_site()
    {
        // arrange
        var source = Startup( "services.AddControllers();\n        services.AddApiVersioning();" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "AddApiVersioning" );
    }

    [Fact]
    public async Task analyzer_should_report_at_every_api_versioning_call_site()
    {
        // arrange
        var controllers = Startup( "services.AddControllers();", "Controllers" );
        var first = Startup( "services.AddApiVersioning();", "First" );
        var second = Startup( "services.AddApiVersioning();", "Second" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( controllers, first, second );

        // assert
        diagnostics.Should().HaveCount( 2 ).And.OnlyContain( diagnostic => diagnostic.Id == AV0013 );
    }

    private static string Startup( string body, string name = "Startup" ) =>
        $$"""
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.Extensions.DependencyInjection;

        public class {{name}}
        {
            public void ConfigureServices( IServiceCollection services )
            {
                {{body}}
            }
        }
        """;
}