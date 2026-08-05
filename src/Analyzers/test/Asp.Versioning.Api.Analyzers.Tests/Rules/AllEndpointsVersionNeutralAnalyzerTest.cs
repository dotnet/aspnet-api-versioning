// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class AllEndpointsVersionNeutralAnalyzerTest
{
    private const string AV0018 = nameof( AV0018 );

    [Fact]
    public async Task analyzer_should_report_when_every_controller_is_version_neutral()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [ApiVersionNeutral]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [ApiController]
            [ApiVersionNeutral]
            [Route( "api/[controller]" )]
            public class PeopleController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0018 );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_a_version_is_declared_anywhere()
    {
        // arrange
        // one explicit version gives the API explorer something to describe the neutral endpoint against
        var source = Controllers( """
            [ApiController]
            [ApiVersionNeutral]
            [Route( "api/[controller]" )]
            public class HealthController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [ApiController]
            [ApiVersion( 1.0 )]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_when_an_endpoint_declares_nothing()
    {
        // arrange
        // an endpoint without any metadata is a different problem, reported by another rule
        var source = Controllers( """
            [ApiController]
            [ApiVersionNeutral]
            [Route( "api/[controller]" )]
            public class HealthController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [ApiController]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_when_an_action_is_version_neutral()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                [ApiVersionNeutral]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0018 );
    }

    [Fact]
    public async Task analyzer_should_report_when_every_minimal_api_is_version_neutral()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/orders", () => "" ).IsApiVersionNeutral();
            app.MapGet( "/api/people", () => "" ).IsApiVersionNeutral();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0018 );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_a_minimal_api_declares_a_version()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/health", () => "" ).IsApiVersionNeutral();
            app.MapGet( "/api/orders", () => "" ).HasApiVersion( 1.0 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_when_a_group_is_version_neutral()
    {
        // arrange
        var source = Application( """
            var api = app.MapGroup( "/api" ).IsApiVersionNeutral();

            api.MapGet( "/orders", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0018 );
    }

    [Fact]
    public async Task analyzer_should_not_report_without_any_endpoints()
    {
        // arrange
        var source = Application( string.Empty );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_without_api_versioning()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Mvc;

            [ApiController]
            [ApiVersionNeutral]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_when_a_route_cannot_be_followed()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Routing;
            using Microsoft.Extensions.DependencyInjection;

            public static class Startup
            {
                public static void ConfigureServices( IServiceCollection services ) => services.AddApiVersioning();

                public static void MapOrders( IEndpointRouteBuilder builder ) =>
                    builder.MapGet( "/orders", () => "" ).IsApiVersionNeutral();
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_at_the_api_versioning_call_site_as_an_error()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/orders", () => "" ).IsApiVersionNeutral();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "AddApiVersioning" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Error );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( string source ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( source ) ).Where( diagnostic => diagnostic.Id == AV0018 )];

    private static string Controllers( string controllers ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.Extensions.DependencyInjection;

        public static class Startup
        {
            public static void ConfigureServices( IServiceCollection services ) => services.AddApiVersioning();
        }

        {{controllers}}
        """;

    private static string Application( string endpoints ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Routing;
        using Microsoft.Extensions.DependencyInjection;

        public static class Startup
        {
            public static void ConfigureServices( IServiceCollection services ) => services.AddApiVersioning();

            public static void Configure( WebApplication app )
            {
                {{endpoints}}
            }
        }
        """;
}