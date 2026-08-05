// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class VersionedAndNeutralAnalyzerTest
{
    private const string AV0019 = nameof( AV0019 );

    [Fact]
    public async Task analyzer_should_not_report_a_version_neutral_action()
    {
        // arrange
        // an action states something more explicit than the controller, which is the intended use
        var source = Controllers( """
            [ApiController]
            [ApiVersion( 1.0 )]
            [ApiVersion( 2.0 )]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();

                [HttpDelete( "{id}" )]
                [ApiVersionNeutral]
                public IActionResult Delete( int id ) => NoContent();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_a_versioned_action_of_a_neutral_controller()
    {
        // arrange
        // the controller has no versions at all, so an action cannot claim one
        var source = Controllers( """
            [ApiController]
            [ApiVersionNeutral]
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
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0019 );
    }

    [Fact]
    public async Task analyzer_should_report_a_controller_declaring_both()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [ApiVersion( 1.0 )]
            [ApiVersionNeutral]
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
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0019 );
    }

    [Fact]
    public async Task analyzer_should_report_an_action_declaring_both()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [Route( "api/[controller]" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                [ApiVersion( 1.0 )]
                [ApiVersionNeutral]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0019 );
    }

    [Fact]
    public async Task analyzer_should_report_across_a_collated_controller()
    {
        // arrange
        // both collate to Orders, so the neutral declaration silences the version on the other
        var source = Controllers( """
            [ApiController]
            [ApiVersionNeutral]
            [Route( "api/orders" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [ApiController]
            [ApiVersion( 2.0 )]
            [Route( "api/v{version:apiVersion}/orders" )]
            public class Orders2Controller : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0019 );
    }

    [Fact]
    public async Task analyzer_should_not_report_versions_across_collated_controllers()
    {
        // arrange
        // versioning the same API from more than one class is ordinary
        var source = Controllers( """
            [ApiController]
            [ApiVersion( 1.0 )]
            [Route( "api/v{version:apiVersion}/orders" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [ApiController]
            [ApiVersion( 2.0 )]
            [Route( "api/v{version:apiVersion}/orders" )]
            public class Orders2Controller : ControllerBase
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
    public async Task analyzer_should_not_report_controllers_that_do_not_collate()
    {
        // arrange
        // a neutral API alongside a separate versioned API is entirely reasonable
        var source = Controllers( """
            [ApiController]
            [ApiVersionNeutral]
            [Route( "api/health" )]
            public class HealthController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [ApiController]
            [ApiVersion( 1.0 )]
            [Route( "api/v{version:apiVersion}/orders" )]
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
    public async Task analyzer_should_collate_by_an_explicit_controller_name()
    {
        // arrange
        // the declared name overrides the type name, so these collate despite not looking alike
        var source = Controllers( """
            [ApiController]
            [ControllerName( "Orders" )]
            [ApiVersionNeutral]
            [Route( "api/orders" )]
            public class LegacyController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }

            [ApiController]
            [ApiVersion( 2.0 )]
            [Route( "api/v{version:apiVersion}/orders" )]
            public class OrdersController : ControllerBase
            {
                [HttpGet]
                public IActionResult Get() => Ok();
            }
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0019 );
    }

    [Theory]
    [InlineData( "services.AddSingleton<IControllerNameConvention, OriginalControllerNameConvention>();" )]
    [InlineData( "services.AddSingleton( ControllerNameConvention.Original );" )]
    [InlineData( "services.AddSingleton<IControllerNameConvention, CustomNameConvention>();" )]
    public async Task analyzer_should_not_collate_under_an_unrecognized_name_convention( string registration )
    {
        // arrange
        // collation follows the naming convention, and a replacement decides it by other rules
        var source = Collated( registration );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "services.AddSingleton<IControllerNameConvention, DefaultControllerNameConvention>();" )]
    [InlineData( "services.AddSingleton( ControllerNameConvention.Grouped );" )]
    public async Task analyzer_should_collate_under_a_trimming_name_convention( string registration )
    {
        // arrange
        // these are the conventions that trim trailing numbers, which is what is reproduced here
        var source = Collated( registration );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0019 );
    }

    [Fact]
    public async Task analyzer_should_report_a_versioned_endpoint_of_a_neutral_group()
    {
        // arrange
        var source = Application( """
            var api = app.NewVersionedApi().IsApiVersionNeutral();

            api.MapGet( "/orders", () => "" ).HasApiVersion( 1.0 );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0019 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_neutral_endpoint_of_a_versioned_group()
    {
        // arrange
        // the endpoint states something more explicit than the group, which is the intended use
        var source = Application( """
            var api = app.NewVersionedApi().HasApiVersion( 1.0 );

            api.MapGet( "/orders", () => "" );
            api.MapDelete( "/orders/{id}", ( int id ) => "" ).IsApiVersionNeutral();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_an_endpoint_declaring_both()
    {
        // arrange
        var source = Application( """
            app.MapGet( "/orders", () => "" ).HasApiVersion( 1.0 ).IsApiVersionNeutral();
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0019 );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_a_group_cannot_be_followed()
    {
        // arrange
        var source = """
            using Asp.Versioning;
            using Asp.Versioning.Builder;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Routing;

            public static class Startup
            {
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
    public async Task analyzer_should_report_at_the_declared_version_as_an_error()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            [ApiVersionNeutral]
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
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "ApiVersion( 1.0 )" );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Error );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( string source ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( source ) ).Where( diagnostic => diagnostic.Id == AV0019 )];

    private static string Collated( string registration ) =>
        $$"""
        using Asp.Versioning;
        using Asp.Versioning.Conventions;
        using Microsoft.AspNetCore.Mvc;
        using Microsoft.AspNetCore.Mvc.ApplicationModels;
        using Microsoft.Extensions.DependencyInjection;

        public class CustomNameConvention : IControllerNameConvention
        {
            public string NormalizeName( string controllerName ) => controllerName;

            public string GroupName( string controllerName ) => controllerName;
        }

        public static class Startup
        {
            public static void ConfigureServices( IServiceCollection services )
            {
                {{registration}}
                services.AddApiVersioning();
            }
        }

        [ApiController]
        [ApiVersionNeutral]
        [Route( "api/orders" )]
        public class OrdersController : ControllerBase
        {
            [HttpGet]
            public IActionResult Get() => Ok();
        }

        [ApiController]
        [ApiVersion( 2.0 )]
        [Route( "api/v{version:apiVersion}/orders" )]
        public class Orders2Controller : ControllerBase
        {
            [HttpGet]
            public IActionResult Get() => Ok();
        }
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

    private static string Application( string endpoints ) =>
        $$"""
        using Asp.Versioning;
        using Asp.Versioning.Builder;
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