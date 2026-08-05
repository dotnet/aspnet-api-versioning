// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class SpecificApiVersionReaderAnalyzerTest
{
    private const string AV0015 = nameof( AV0015 );

    [Fact]
    public async Task analyzer_should_report_url_segment_for_minimal_apis()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/v{version:apiVersion}/people", () => "" );
            app.MapGet( "/api/v{version:apiVersion}/orders", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;

        diagnostic.Id.Should().Be( AV0015 );
        diagnostic.GetMessage().Should().Contain( "UrlSegmentApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_report_query_string_for_minimal_apis()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/people", () => "" );
            app.MapGet( "/api/orders", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;

        diagnostic.Id.Should().Be( AV0015 );
        diagnostic.GetMessage().Should().Contain( "QueryStringApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_mixture_of_styles()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/v{version:apiVersion}/people", () => "" );
            app.MapGet( "/api/orders", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_resolve_a_group_prefix_through_a_chain()
    {
        // arrange
        var source = Application( """
            app.MapGroup( "/api/v{version:apiVersion}" ).MapGet( "/people", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "UrlSegmentApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_resolve_a_group_prefix_through_a_local()
    {
        // arrange
        // the shape used by the minimal API examples, where the group flows through a fluent chain
        var source = Application( """
            var api = app.MapGroup( "/api/v{version:apiVersion}/people" )
                         .HasApiVersion( 1.0 );

            api.MapGet( "/{id:int}", () => "" );
            api.MapGet( "/", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "UrlSegmentApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_a_route_cannot_be_followed()
    {
        // arrange
        // the prefix comes from the caller, so the template seen here may be missing the constraint
        var source = """
            using Asp.Versioning;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Routing;
            using Microsoft.Extensions.DependencyInjection;

            public static class Startup
            {
                public static void Configure( IServiceCollection services ) => services.AddApiVersioning();

                public static void MapPeople( IEndpointRouteBuilder builder ) =>
                    builder.MapGet( "/people", () => "" );
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_when_a_template_is_not_constant()
    {
        // arrange
        const string Endpoints = """
            var route = GetRoute();

            app.MapGet( route, () => "" );
            """;
        const string Members = """
            private static string GetRoute() => "/api/people";
            """;

        var source = Application( Endpoints, Members );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_when_the_reader_is_configured()
    {
        // arrange
        var source = Application(
            """
            app.MapGet( "/api/people", () => "" );
            """,
            configure: "services.AddApiVersioning( options => options.ApiVersionReader = new QueryStringApiVersionReader() );" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_without_any_endpoints()
    {
        // arrange
        var source = Application( string.Empty );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_without_api_versioning()
    {
        // arrange
        var source = """
            using Microsoft.AspNetCore.Builder;

            public static class Startup
            {
                public static void Configure( WebApplication app ) =>
                    app.MapGet( "/api/people", () => "" );
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_ignore_a_version_neutral_endpoint()
    {
        // arrange
        // the neutral endpoint has no constraint, but must not count as a mixture
        var source = Application( """
            app.MapGet( "/api/v{version:apiVersion}/people", () => "" );
            app.MapGet( "/health", () => "" ).IsApiVersionNeutral();
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "UrlSegmentApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_honor_a_configured_constraint_name()
    {
        // arrange
        var source = Application(
            """
            app.MapGet( "/api/v{version:ver}/people", () => "" );
            """,
            configure: "services.AddApiVersioning( options => options.RouteConstraintName = \"ver\" );" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "UrlSegmentApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_not_recognize_the_default_constraint_name_when_reconfigured()
    {
        // arrange
        var source = Application(
            """
            app.MapGet( "/api/v{version:apiVersion}/people", () => "" );
            """,
            configure: "services.AddApiVersioning( options => options.RouteConstraintName = \"ver\" );" );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "QueryStringApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_report_url_segment_for_controllers()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [Route( "api/v{version:apiVersion}/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "UrlSegmentApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_report_query_string_for_controllers()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "QueryStringApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_ignore_a_user_interface_controller()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [Route( "api/v{version:apiVersion}/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [Route( "home" )]
            public class HomeController : Controller
            {
                [HttpGet]
                public IActionResult Index() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "UrlSegmentApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_ignore_a_version_neutral_controller()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [Route( "api/v{version:apiVersion}/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [ApiVersionNeutral]
            [ApiController]
            [Route( "health" )]
            public class HealthController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.GetMessage().Should().Contain( "UrlSegmentApiVersionReader" );
    }

    [Theory]
    [InlineData( "options.AddRouteComponents( \"api/v{version:apiVersion}\" )", "UrlSegmentApiVersionReader" )]
    [InlineData( "options.AddRouteComponents( \"api\" )", "QueryStringApiVersionReader" )]
    [InlineData( "options.AddRouteComponents()", "QueryStringApiVersionReader" )]
    public async Task analyzer_should_report_for_odata_route_components( string components, string reader )
    {
        // arrange
        var source = ODataApi( components );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should()
                   .ContainSingle( diagnostic => diagnostic.Id == AV0015 )
                   .Which.GetMessage()
                   .Should()
                   .Contain( reader );
    }

    [Fact]
    public async Task analyzer_should_ignore_an_odata_controller()
    {
        // arrange
        // an OData controller is routed by its registered components rather than by an attribute, so
        // counting its template-less actions would look like a mixture of styles
        const string Controller = """
            [ApiController]
            public class OrdersController : ODataController
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """;

        var source = ODataApi( "options.AddRouteComponents( \"api/v{version:apiVersion}\" )", Controller );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should()
                   .ContainSingle( diagnostic => diagnostic.Id == AV0015 )
                   .Which.GetMessage()
                   .Should()
                   .Contain( "UrlSegmentApiVersionReader" );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_api_versioning_call_site()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/people", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "AddApiVersioning" );
    }

    private static string Application(
        string endpoints,
        string members = "",
        string configure = "services.AddApiVersioning();" ) =>
        $$"""
        using System;
        using Asp.Versioning;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Routing;
        using Microsoft.Extensions.DependencyInjection;

        public static class Startup
        {
            public static void ConfigureServices( IServiceCollection services )
            {
                {{configure}}
            }

            public static void Configure( WebApplication app )
            {
                {{endpoints}}
            }

            {{members}}
        }
        """;

    private static string ODataApi( string components, string controllers = "" ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.AspNetCore.OData.Routing.Controllers;
        using Microsoft.Extensions.DependencyInjection;

        public static class Startup
        {
            public static void ConfigureServices( IServiceCollection services ) =>
                services.AddApiVersioning().AddOData( options => {{components}} );
        }

        {{controllers}}
        """;

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
}