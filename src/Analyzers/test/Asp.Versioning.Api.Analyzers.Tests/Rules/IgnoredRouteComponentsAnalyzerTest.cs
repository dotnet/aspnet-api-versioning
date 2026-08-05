// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class IgnoredRouteComponentsAnalyzerTest
{
    private const string AV0023 = nameof( AV0023 );

    [Fact]
    public async Task analyzer_should_report_route_components_configured_for_odata()
    {
        // arrange
        var source = Configured( """
            services.AddControllers().AddOData( options => options.AddRouteComponents( "api", new EdmModel() ) );
            services.AddApiVersioning().AddOData();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0023 );
    }

    [Fact]
    public async Task analyzer_should_report_route_components_configured_by_options()
    {
        // arrange
        // the options can be reached without going through OData itself
        var source = Configured( """
            services.Configure<ODataOptions>( options => options.AddRouteComponents( new EdmModel() ) );
            services.AddApiVersioning().AddOData();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0023 );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_route_components_call_site()
    {
        // arrange
        var source = Configured( """
            services.AddControllers().AddOData( options => options.AddRouteComponents( "api", new EdmModel() ) );
            services.AddApiVersioning().AddOData();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "AddRouteComponents" );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_route_components_are_versioned()
    {
        // arrange
        var source = Configured(
            """services.AddApiVersioning().AddOData( options => options.AddRouteComponents( "api" ) );""" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_when_odata_is_versioned_with_a_setup_action()
    {
        // arrange
        // stating the same prefix in both places collides once the versioned options are resolved
        var source = Configured( """
            services.AddControllers().AddOData( options => options.AddRouteComponents( "api", new EdmModel() ) );
            services.AddApiVersioning().AddOData( options => options.AddRouteComponents( "api" ) );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0023 );
    }

    [Fact]
    public async Task analyzer_should_report_only_the_route_components_configured_for_odata()
    {
        // arrange
        // the versioned options declare AddRouteComponents of their own, which is the correct one
        var source = Configured( """
            services.AddControllers().AddOData( options => options.AddRouteComponents( "api", new EdmModel() ) );
            services.AddApiVersioning().AddOData( options => options.AddRouteComponents( "other" ) );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var location = diagnostics.Should().ContainSingle().Subject.Location;
        var line = location.GetLineSpan().StartLinePosition.Line;

        source.Split( '\n' )[line].Should().Contain( "AddControllers" );
    }

    [Fact]
    public async Task analyzer_should_not_report_without_versioned_odata()
    {
        // arrange
        // nothing replaces the options, so the route components are applied as they are written
        var source = Configured( """
            services.AddControllers().AddOData( options => options.AddRouteComponents( "api", new EdmModel() ) );
            services.AddApiVersioning();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_the_odata_api_explorer()
    {
        // arrange
        // the explorer does not replace the options the way the core services do
        var source = Configured( """
            services.AddControllers().AddOData( options => options.AddRouteComponents( "api", new EdmModel() ) );
            services.AddApiVersioning().AddODataApiExplorer();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_each_route_component()
    {
        // arrange
        var source = Configured( """
            services.AddControllers().AddOData( options =>
            {
                options.AddRouteComponents( "api", new EdmModel() );
                options.AddRouteComponents( "other", new EdmModel() );
            } );
            services.AddApiVersioning().AddOData();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().HaveCount( 2 ).And.OnlyContain( diagnostic => diagnostic.Id == AV0023 );
    }

    [Fact]
    public async Task analyzer_should_report_across_files()
    {
        // arrange
        var odata = Configured(
            """services.AddControllers().AddOData( options => options.AddRouteComponents( "api", new EdmModel() ) );""",
            "Data" );
        var versioning = Configured( "services.AddApiVersioning().AddOData();", "Versioning" );

        // act
        var diagnostics = await AnalyzeAsync( odata, versioning );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0023 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_user_defined_add_route_components()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Microsoft.Extensions.DependencyInjection;

            public class Components
            {
                public void AddRouteComponents( string prefix )
                {
                }
            }

            public static class Startup
            {
                public static void ConfigureServices( IServiceCollection services )
                {
                    new Components().AddRouteComponents( "api" );
                    services.AddApiVersioning().AddOData();
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
            .Where( diagnostic => diagnostic.Id == AV0023 )];

    private static string Configured( string body, string name = "Startup" ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.AspNetCore.OData;
        using Microsoft.Extensions.DependencyInjection;
        using Microsoft.OData.Edm;

        public static class {{name}}
        {
            public static void ConfigureServices( IServiceCollection services )
            {
                {{body}}
            }
        }
        """;
}