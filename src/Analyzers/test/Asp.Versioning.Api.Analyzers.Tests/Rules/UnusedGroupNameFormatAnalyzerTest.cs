// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class UnusedGroupNameFormatAnalyzerTest
{
    private const string AV0026 = nameof( AV0026 );
    private const string Format = "options.FormatGroupName = ( group, version ) => $\"{group}-{version}\"";

    [Fact]
    public async Task analyzer_should_report_a_format_no_api_uses()
    {
        // arrange
        var source = Controller( Format );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0026 );
    }

    [Theory]
    [InlineData( "[ApiExplorerSettings( GroupName = \"orders\" )]" )]
    [InlineData( "[ApiExplorerSettings( IgnoreApi = false, GroupName = \"orders\" )]" )]
    public async Task analyzer_should_not_report_a_group_name_on_a_controller( string attribute )
    {
        // arrange
        var source = Controller( Format, attribute );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "[ApiExplorerSettings( IgnoreApi = true )]" )]
    [InlineData( "[ApiExplorerSettings( GroupName = \"\" )]" )]
    [InlineData( "[ApiExplorerSettings( GroupName = null )]" )]
    public async Task analyzer_should_report_settings_without_a_group_name( string attribute )
    {
        // arrange
        var source = Controller( Format, attribute );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0026 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_group_name_on_a_minimal_api()
    {
        // arrange
        var source = MinimalApi( """app.MapGet( "/order", () => "" ).WithGroupName( "orders" );""" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_group_name_on_a_route_group()
    {
        // arrange
        // a group of endpoints carries the name to every endpoint within it
        var source = MinimalApi( """
            app.MapGet( "/order", () => "" );
            app.MapGroup( "/orders" ).WithGroupName( "orders" );
            """ );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_group_name_attribute_on_a_handler()
    {
        // arrange
        var source = """
            using System;
            using Asp.Versioning.ApiExplorer;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Routing;

            public static class Startup
            {
                public static void Configure( WebApplication app, ApiExplorerOptions options )
                {
                    app.MapGet( "/order", Handler );
                    options.FormatGroupName = ( group, version ) => $"{group}-{version}";
                }

                [EndpointGroupName( "orders" )]
                private static string Handler() => "";
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_a_minimal_api_without_a_group_name()
    {
        // arrange
        var source = MinimalApi( """app.MapGet( "/order", () => "" );""" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0026 );
    }

    [Fact]
    public async Task analyzer_should_not_report_a_single_group_name_among_many_apis()
    {
        // arrange
        // one name anywhere is enough to put the callback to use
        var source = """
            using System;
            using Asp.Versioning.ApiExplorer;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Mvc;

            public class OrdersController : ControllerBase
            {
            }

            [ApiExplorerSettings( GroupName = "people" )]
            public class PeopleController : ControllerBase
            {
            }

            public static class Startup
            {
                public static void Configure( WebApplication app, ApiExplorerOptions options )
                {
                    app.MapGet( "/order", () => "" );
                    options.FormatGroupName = ( group, version ) => $"{group}-{version}";
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_cleared_callback()
    {
        // arrange
        // nothing is reached when there is no callback to reach
        var source = Controller( "options.FormatGroupName = null" );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_group_name_known_only_at_run_time()
    {
        // arrange
        var source = """
            using System;
            using Asp.Versioning.ApiExplorer;
            using Microsoft.AspNetCore.Builder;

            public static class Startup
            {
                public static void Configure( WebApplication app, ApiExplorerOptions options, string name )
                {
                    app.MapGet( "/order", () => "" ).WithGroupName( name );
                    options.FormatGroupName = ( group, version ) => $"{group}-{version}";
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_when_group_names_come_from_elsewhere()
    {
        // arrange
        // a provider of its own says nothing about whether any names are set
        var source = """
            using System;
            using Asp.Versioning.ApiExplorer;
            using Microsoft.AspNetCore.Builder;
            using Microsoft.AspNetCore.Mvc.ApiExplorer;

            [AttributeUsage( AttributeTargets.Class )]
            public sealed class TenantAttribute : Attribute, IApiDescriptionGroupNameProvider
            {
                public string GroupName => "tenant";
            }

            public static class Startup
            {
                public static void Configure( WebApplication app, ApiExplorerOptions options )
                {
                    app.MapGet( "/order", () => "" );
                    options.FormatGroupName = ( group, version ) => $"{group}-{version}";
                }
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_when_no_apis_are_declared()
    {
        // arrange
        // an application whose APIs are declared elsewhere keeps its group names there as well
        var source = """
            using System;
            using Asp.Versioning.ApiExplorer;

            public static class Startup
            {
                public static void Configure( ApiExplorerOptions options ) =>
                    options.FormatGroupName = ( group, version ) => $"{group}-{version}";
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_across_files()
    {
        // arrange
        var controller = """
            using Microsoft.AspNetCore.Mvc;

            public class OrdersController : ControllerBase
            {
            }
            """;
        var startup = """
            using System;
            using Asp.Versioning.ApiExplorer;

            public static class Startup
            {
                public static void Configure( ApiExplorerOptions options ) =>
                    options.FormatGroupName = ( group, version ) => $"{group}-{version}";
            }
            """;

        // act
        var diagnostics = await AnalyzeAsync( controller, startup );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0026 );
    }

    [Fact]
    public async Task analyzer_should_report_across_the_assignment_as_unnecessary_code()
    {
        // arrange
        var source = Controller( Format );

        // act
        var diagnostics = await AnalyzeAsync( source );

        // assert
        var diagnostic = diagnostics.Should().ContainSingle().Subject;
        var span = diagnostic.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( Format );
        diagnostic.Severity.Should().Be( DiagnosticSeverity.Info );
        diagnostic.Descriptor.CustomTags.Should().Contain( WellKnownDiagnosticTags.Unnecessary );
    }

    // other rules can legitimately apply to the same configuration, so each test is scoped to its own
    private static async Task<IReadOnlyList<Diagnostic>> AnalyzeAsync( params string[] sources ) =>
        [.. ( await AnalyzerVerifier.AnalyzeAsync( sources ) ).Where( diagnostic => diagnostic.Id == AV0026 )];

    private static string Controller( string assignment, string attribute = "" ) =>
        $$"""
        using System;
        using Asp.Versioning.ApiExplorer;
        using Microsoft.AspNetCore.Mvc;

        {{attribute}}
        public class OrdersController : ControllerBase
        {
        }

        public static class Startup
        {
            public static void Configure( ApiExplorerOptions options ) => {{assignment}};
        }
        """;

    private static string MinimalApi( string body ) =>
        $$"""
        using System;
        using Asp.Versioning.ApiExplorer;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Routing;

        public static class Startup
        {
            public static void Configure( WebApplication app, ApiExplorerOptions options )
            {
                {{body}}
                options.FormatGroupName = ( group, version ) => $"{group}-{version}";
            }
        }
        """;
}