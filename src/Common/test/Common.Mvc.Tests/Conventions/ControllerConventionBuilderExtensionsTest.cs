// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using System.Web.Http;
using ControllerBase = System.Web.Http.ApiController;
using IActionResult = System.Web.Http.IHttpActionResult;
#else
using Microsoft.AspNetCore.Mvc;
#endif
using static Moq.Times;

public class ControllerConventionBuilderExtensionsTest
{
    [Fact]
    public void action_should_map_method_from_action_delegate_expression()
    {
        // arrange
        var method = typeof( StubController ).GetMethod( nameof( StubController.Delete ) );
        var builder = new Mock<ControllerApiVersionConventionBuilder<StubController>>();

        // act
        builder.Object.Action( c => c.Delete() );

        // assert
        builder.Verify( b => b.Action( method ), Once() );
    }

    [Fact]
    public void action_should_map_method_from_func_delegate_expression()
    {
        // arrange
        var method = typeof( StubController ).GetMethod( nameof( StubController.Get ) );
        var builder = new Mock<ControllerApiVersionConventionBuilder<StubController>>();

        // act
        builder.Object.Action( c => c.Get() );

        // assert
        builder.Verify( b => b.Action( method ), Once() );
    }

    [Fact]
    public void action_should_throw_exception_when_func_delegate_expression_is_not_a_method()
    {
        // arrange
        var builder = new Mock<ControllerApiVersionConventionBuilder<StubController>>().Object;

        // act
        Action action = () => builder.Action( c => c.Timeout );

        // assert
        action.Should().Throw<InvalidOperationException>().And
              .Message.Should().Be( "The expression 'c => c.Timeout' must refer to a controller action method." );
    }

    [Fact]
    public void action_should_map_method_from_name()
    {
        // arrange
        const string methodName = nameof( StubController.Post );
        var controllerType = typeof( StubController );
        var method = controllerType.GetMethods().Single( m => m.Name == methodName && m.GetParameters().Length == 0 );
        var builder = new Mock<ControllerApiVersionConventionBuilder>( controllerType );

        // act
        builder.Object.Action( methodName );

        // assert
        builder.Verify( b => b.Action( method ), Once() );
    }

    [Fact]
    public void action_should_map_method_from_name_and_argument_type()
    {
        // arrange
        const string methodName = nameof( StubController.Post );
        var controllerType = typeof( StubController );
        var method = controllerType.GetMethods().Single( m => m.Name == methodName && m.GetParameters().Length == 1 );
        var builder = new Mock<ControllerApiVersionConventionBuilder>( controllerType );

        // act
        builder.Object.Action( methodName, typeof( int ) );

        // assert
        builder.Verify( b => b.Action( method ), Once() );
    }

    [Fact]
    public void action_should_throw_exception_when_method_does_not_exist()
    {
        // arrange
        var message = "An action method with the name 'NoSuchMethod' could not be found. The method must be public, non-static, and not have the NonActionAttribute applied.";
        var builder = new Mock<ControllerApiVersionConventionBuilder>( typeof( StubController ) );

        // act
        Action actionConvention = () => builder.Object.Action( "NoSuchMethod" );

        // assert
        actionConvention.Should().Throw<MissingMethodException>().And.Message.Should().Be( message );
    }

#pragma warning disable IDE0060
#pragma warning disable IDE0079
#pragma warning disable CA1822
#pragma warning disable CA1034 // Nested types should not be visible

#if !NETFRAMEWORK
    [ApiController]
#endif
    public sealed class StubController : ControllerBase
    {
        public IActionResult Get() => Ok();

        public void Delete() { }

        public TimeSpan Timeout { get; set; }

        public IActionResult Post() => Post( 42, "stubs/42" );

        public IActionResult Post( int id ) => Ok();

        [NonAction]
        public IActionResult Post( int id, string location ) => Created( location, new { id } );
    }
}