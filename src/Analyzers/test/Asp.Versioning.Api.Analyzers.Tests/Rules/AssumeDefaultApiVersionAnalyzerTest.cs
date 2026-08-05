// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class AssumeDefaultApiVersionAnalyzerTest
{
    private const string AV0016 = nameof( AV0016 );

    private const string VersionedController = """
        [ApiController]
        [ApiVersion( 1.0 )]
        [Route( "api/[controller]" )]
        public class OrdersController : ControllerBase
        {
            [HttpGet]
            public IActionResult Get() => Ok();
        }
        """;

    [Fact]
    public async Task analyzer_should_report_when_every_endpoint_declares_a_version()
    {
        // arrange
        var source = Controllers( """
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
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0016 );
    }

    [Theory]
    [InlineData( "new MediaTypeApiVersionReader()" )]
    [InlineData( """new MediaTypeApiVersionReader( "v" )""" )]
    [InlineData( """ApiVersionReader.Combine( new QueryStringApiVersionReader(), new MediaTypeApiVersionReader() )""" )]
    [InlineData( """ApiVersionReader.Combine( new MediaTypeApiVersionReader(), new HeaderApiVersionReader( "api-version" ) )""" )]
    [InlineData( """new MediaTypeApiVersionReaderBuilder().Parameter( "v" ).Build()""" )]
    public async Task analyzer_should_not_report_when_the_version_is_read_from_the_media_type( string reader )
    {
        // arrange
        // a client asking for "application/json" has named no version and never will, so assuming a
        // default is what keeps it working
        var source = Controllers( VersionedController, Reading( reader ) );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( """new HeaderApiVersionReader( "api-version" )""" )]
    [InlineData( "new QueryStringApiVersionReader()" )]
    [InlineData( """ApiVersionReader.Combine( new QueryStringApiVersionReader(), new UrlSegmentApiVersionReader() )""" )]
    public async Task analyzer_should_report_when_the_version_is_read_from_elsewhere( string reader )
    {
        // arrange
        // every other reader requires a client to name the version it wants
        var source = Controllers( VersionedController, Reading( reader ) );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0016 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_reader_that_cannot_be_decided()
    {
        // arrange
        // a reader that cannot be read as written may well be reading the media type
        var source = """
            using System.Collections.Generic;
            using Asp.Versioning;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.Extensions.DependencyInjection;

            public static class Startup
            {
                private static IEnumerable<IApiVersionReader> Readers => new IApiVersionReader[0];

                public static void ConfigureServices( IServiceCollection services ) =>
                    services.AddApiVersioning(
                        options =>
                        {
                            options.AssumeDefaultVersionWhenUnspecified = true;
                            options.ApiVersionReader = ApiVersionReader.Combine( Readers );
                        } );
            }

            [ApiController]
            [ApiVersion( 1.0 )]
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
    public async Task analyzer_should_not_report_when_an_endpoint_declares_nothing()
    {
        // arrange
        // the unversioned endpoint is reachable without a version, which is what the default is for
        var source = Controllers( """
            [ApiController]
            [ApiVersion( 1.0 )]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [ApiController]
            [Route( "api/[controller]" )]
            public class LegacyController : ControllerBase
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
    public async Task analyzer_should_report_when_an_endpoint_is_version_neutral()
    {
        // arrange
        // declaring neutrality is still declaring something, so the default cannot apply
        var source = Controllers( """
            [ApiController]
            [ApiVersionNeutral]
            [Route( "api/[controller]" )]
            public class HealthController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0016 );
    }

    [Fact]
    public async Task analyzer_should_report_when_only_an_action_declares_a_version()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                [ApiVersion( 1.0 )]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0016 );
    }

    [Fact]
    public async Task analyzer_should_report_when_every_route_is_constrained()
    {
        // arrange
        // nothing declares a version, but nothing can be reached without naming one either
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
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0016 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_dual_route_registration()
    {
        // arrange
        // the pair is how a default version is applied to a URL segment, so it is deliberate
        var source = Controllers( """
            [ApiController]
            [Route( "api/[controller]" )]
            [Route( "api/v{version:apiVersion}/[controller]" )]
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
    public async Task analyzer_should_not_report_a_dual_route_registration_when_versioned()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [ApiVersion( 1.0 )]
            [Route( "api/[controller]" )]
            [Route( "api/v{version:apiVersion}/[controller]" )]
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
    public async Task analyzer_should_not_report_when_the_setting_is_absent()
    {
        // arrange
        var source = Controllers(
            """
            [ApiController]
            [ApiVersion( 1.0 )]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """,
            configure: "services.AddApiVersioning();" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_when_the_setting_is_false()
    {
        // arrange
        var source = Controllers(
            """
            [ApiController]
            [ApiVersion( 1.0 )]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """,
            configure: "services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = false );" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_at_the_assignment()
    {
        // arrange
        var source = Controllers( """
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
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length )
              .Should()
              .Be( "options.AssumeDefaultVersionWhenUnspecified = true" );
        diagnostic.Descriptor.CustomTags.Should().Contain( WellKnownDiagnosticTags.Unnecessary );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_a_convention_is_unrecognized()
    {
        // arrange
        // a convention that is not understood may version anything, so nothing can be concluded
        var source = """
            using Asp.Versioning;
            using Asp.Versioning.Conventions;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.ApplicationModels;
            using Microsoft.Extensions.DependencyInjection;

            public class CustomConvention : IControllerConvention
            {
                public bool Apply( IControllerConventionBuilder builder, ControllerModel controller ) => true;
            }

            public static class Startup
            {
                public static void ConfigureServices( IServiceCollection services ) =>
                    services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true )
                            .AddMvc( options => options.Conventions.Add( new CustomConvention() ) );
            }

            [ApiController]
            [ApiVersion( 1.0 )]
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
    public async Task analyzer_should_report_when_a_namespace_declares_the_version()
    {
        // arrange
        // the convention is understood, so the namespace versions the controller
        var source = VersionedByNamespace( "Api.v1.Controllers" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0016 );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_a_namespace_declares_nothing()
    {
        // arrange
        var source = VersionedByNamespace( "Api.Controllers" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    private static string VersionedByNamespace( string @namespace ) =>
        $$"""
        using Asp.Versioning;
        using Asp.Versioning.Conventions;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.Extensions.DependencyInjection;

        public static class Startup
        {
            public static void ConfigureServices( IServiceCollection services ) =>
                services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true )
                        .AddMvc( options => options.Conventions.Add( new VersionByNamespaceConvention() ) );
        }

        namespace {{@namespace}}
        {
            [ApiController]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
        }
        """;

    [Fact]
    public async Task analyzer_should_report_for_versioned_minimal_apis()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/orders", () => "" ).HasApiVersion( 1.0 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0016 );
    }

    [Fact]
    public async Task analyzer_should_not_report_for_an_unversioned_minimal_api()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/orders", () => "" ).HasApiVersion( 1.0 );
            app.MapGet( "/api/legacy", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_when_a_group_declares_the_version()
    {
        // arrange
        var source = Application( """
            var api = app.MapGroup( "/api" ).HasApiVersion( 1.0 );

            api.MapGet( "/orders", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0016 );
    }

    [Fact]
    public async Task analyzer_should_report_for_a_constrained_minimal_api()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/api/v{version:apiVersion}/orders", () => "" );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0016 );
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
                public static void ConfigureServices( IServiceCollection services ) =>
                    services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true );

                public static void MapOrders( IEndpointRouteBuilder builder ) =>
                    builder.MapGet( "/orders", () => "" ).HasApiVersion( 1.0 );
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_without_any_endpoints()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Microsoft.Extensions.DependencyInjection;

            public static class Startup
            {
                public static void ConfigureServices( IServiceCollection services ) =>
                    services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true );
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    // both rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( string source ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( source ) ).Where( diagnostic => diagnostic.Id == AV0016 )];

    private static string Reading( string reader ) =>
        $$"""
        services.AddApiVersioning(
                    options =>
                    {
                        options.AssumeDefaultVersionWhenUnspecified = true;
                        options.ApiVersionReader = {{reader}};
                    } );
        """;

    private static string Controllers(
        string controllers,
        string configure =
            "services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true );" ) =>
        $$"""
        using Asp.Versioning;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.Extensions.DependencyInjection;

        public static class Startup
        {
            public static void ConfigureServices( IServiceCollection services ) => {{configure}}
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
            public static void ConfigureServices( IServiceCollection services ) =>
                services.AddApiVersioning( options => options.AssumeDefaultVersionWhenUnspecified = true );

            public static void Configure( WebApplication app )
            {
                {{endpoints}}
            }
        }
        """;
}