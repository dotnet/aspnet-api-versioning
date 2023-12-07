// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dtime

namespace Asp.Versioning.Conventions;

#if NETFRAMEWORK
using Microsoft.AspNet.OData;
using IActionResult = System.Web.Http.IHttpActionResult;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Controllers;
#endif

public partial class ODataQueryOptionsConventionBuilderTest
{
    [Fact]
    public void controller_should_add_new_controller_builder_conventions()
    {
        // arrange
        var builder = new TestODataQueryOptionsConventionBuilder();

        // act
        var controllerBuilder = builder.Controller<StubController>();

        // assert
        builder.ConventionBuilders.Should().BeEquivalentTo(
            new Dictionary<Type, IODataQueryOptionsConventionBuilder>()
            {
                [typeof( StubController )] = controllerBuilder,
            } );
    }

    [Fact]
    public void controller_should_add_new_controller_builder_conventions_for_type()
    {
        // arrange
        var builder = new TestODataQueryOptionsConventionBuilder();

        // act
        var controllerBuilder = builder.Controller( typeof( StubController ) );

        // assert
        builder.ConventionBuilders.Should().BeEquivalentTo(
            new Dictionary<Type, IODataQueryOptionsConventionBuilder>()
            {
                [typeof( StubController )] = controllerBuilder,
            } );
    }

    [Fact]
    public void controller_should_return_existing_controller_builder_conventions()
    {
        // arrange
        var builder = new ODataQueryOptionsConventionBuilder();
        var originalControllerBuilder = builder.Controller<StubController>();

        // act
        var controllerBuilder = builder.Controller<StubController>();

        // assert
        controllerBuilder.Should().BeSameAs( originalControllerBuilder );
    }

    [Fact]
    public void controller_should_return_existing_controller_builder_conventions_for_type()
    {
        // arrange
        var builder = new ODataQueryOptionsConventionBuilder();
        var originalControllerBuilder = builder.Controller( typeof( StubController ) );

        // act
        var controllerBuilder = builder.Controller( typeof( StubController ) );

        // assert
        controllerBuilder.Should().BeSameAs( originalControllerBuilder );
    }

    [Fact]
    public void controller_should_not_allow_both_compileX2Dtime_and_runX2Dtime_conventions()
    {
        // arrange
        var builder = new ODataQueryOptionsConventionBuilder();

        builder.Controller<StubController>();

        // act
        Action controllerConvention = () => builder.Controller( typeof( StubController ) );

        // assert
        controllerConvention.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void controller_for_type_should_not_allow_both_compileX2Dtime_and_runX2Dtime_conventions()
    {
        // arrange
        var builder = new ODataQueryOptionsConventionBuilder();

        builder.Controller( typeof( StubController ) );

        // act
        Action controllerConvention = () => builder.Controller<StubController>();

        // assert
        controllerConvention.Should().Throw<InvalidOperationException>();
    }

    private sealed class TestODataQueryOptionsConventionBuilder : ODataQueryOptionsConventionBuilder
    {
        internal new IDictionary<Type, IODataQueryOptionsConventionBuilder> ConventionBuilders => base.ConventionBuilders;

        internal new IList<IODataQueryOptionsConvention> Conventions => base.Conventions;
    }

#pragma warning disable IDE0079
#pragma warning disable CA1034 // Nested types should not be visible

    public sealed class StubController : ODataController
    {
        public IActionResult Get() => Ok();
    }
}