// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Analyzers.Rules;

public class MissingApiBehaviorAnalyzerTest
{
    private const string AV0014 = nameof( AV0014 );

    [Fact]
    public async Task analyzer_should_report_controller_without_api_behavior()
    {
        // arrange
        var source = Controllers( """
            public class OrdersController : ControllerBase
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0014 );
    }

    [Fact]
    public async Task analyzer_should_not_report_controller_with_api_behavior()
    {
        // arrange
        var source = Controllers( """
            [ApiController]
            public class OrdersController : ControllerBase
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Theory]
    [InlineData( "[assembly: ApiController]" )]
    [InlineData( "[assembly: Microsoft.AspNetCore.Mvc.ApiController]" )]
    public async Task analyzer_should_not_report_when_api_behavior_is_applied_to_the_assembly( string attribute )
    {
        // arrange
        // the second form is what a build generates from an AssemblyAttribute item
        var source = $$"""
            using Microsoft.AspNetCore.Mvc;

            {{attribute}}

            public class OrdersController : ControllerBase
            {
            }

            public class PeopleController : ControllerBase
            {
            }
            """;

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_when_the_assembly_applies_api_behavior_in_another_file()
    {
        // arrange
        var attribute = """
            [assembly: Microsoft.AspNetCore.Mvc.ApiController]
            """;
        var controller = Controllers( """
            public class OrdersController : ControllerBase
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( attribute, controller );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_user_interface_controller()
    {
        // arrange
        // Controller extends ControllerBase, but serves views rather than an API
        var source = Controllers( """
            public class HomeController : Controller
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_through_a_user_interface_base_class()
    {
        // arrange
        var source = Controllers( """
            public abstract class UserInterfaceController : Controller
            {
            }

            public class HomeController : UserInterfaceController
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_through_a_user_defined_base_class()
    {
        // arrange
        var source = Controllers( """
            public abstract class ApiControllerBase : ControllerBase
            {
            }

            public class OrdersController : ApiControllerBase
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().ContainSingle().Which.Id.Should().Be( AV0014 );
    }

    [Fact]
    public async Task analyzer_should_not_report_when_a_base_class_applies_api_behavior()
    {
        // arrange
        // the attribute is inherited, so the base class applies it for every controller under it
        var source = Controllers( """
            [ApiController]
            public abstract class ApiControllerBase : ControllerBase
            {
            }

            public class OrdersController : ApiControllerBase
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_an_abstract_controller()
    {
        // arrange
        var source = Controllers( """
            public abstract class ApiControllerBase : ControllerBase
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_nested_controller()
    {
        // arrange
        var source = Controllers( """
            public static class Outer
            {
                public class OrdersController : ControllerBase
                {
                }
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_not_report_a_class_that_is_not_a_controller()
    {
        // arrange
        var source = Controllers( """
            public class Orders
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().BeEmpty();
    }

    [Fact]
    public async Task analyzer_should_report_each_controller_without_api_behavior()
    {
        // arrange
        var source = Controllers( """
            public class OrdersController : ControllerBase
            {
            }

            [ApiController]
            public class PeopleController : ControllerBase
            {
            }

            public class BooksController : ControllerBase
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        diagnostics.Should().HaveCount( 2 ).And.OnlyContain( diagnostic => diagnostic.Id == AV0014 );
    }

    [Fact]
    public async Task analyzer_should_report_at_the_controller_name()
    {
        // arrange
        var source = Controllers( """
            public class OrdersController : ControllerBase
            {
            }
            """ );

        // act
        var diagnostics = await AnalyzerVerifier.AnalyzeAsync( source );

        // assert
        var span = diagnostics.Should().ContainSingle().Subject.Location.SourceSpan;

        source.Substring( span.Start, span.Length ).Should().Be( "OrdersController" );
    }

    private static string Controllers( string body ) =>
        $$"""
        using Microsoft.AspNetCore.Mvc;

        {{body}}
        """;
}