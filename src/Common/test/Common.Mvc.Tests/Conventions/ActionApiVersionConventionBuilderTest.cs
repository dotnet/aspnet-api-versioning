// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Conventions;

#if !NETFRAMEWORK
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
#endif
#if NETFRAMEWORK
using ActionModel = System.Web.Http.Controllers.HttpActionDescriptor;
using ControllerBase = System.Web.Http.ApiController;
using IActionResult = System.Web.Http.IHttpActionResult;
#endif
using System.Reflection;
using static Moq.Times;

public partial class ActionApiVersionConventionBuilderTest
{
    [Fact]
    public void action_should_call_action_on_controller_builder()
    {
        // arrange
        var controllerBuilder = new Mock<ControllerApiVersionConventionBuilder>( typeof( UndecoratedController ) );
        var actionBuilder = new ActionApiVersionConventionBuilder( controllerBuilder.Object );
        var method = typeof( UndecoratedController ).GetMethod( nameof( UndecoratedController.Get ) );

        controllerBuilder.Setup( cb => cb.Action( It.IsAny<MethodInfo>() ) );

        // act
        actionBuilder.Action( method );

        // assert
        controllerBuilder.Verify( cb => cb.Action( method ), Once() );
    }

    [Fact]
    public void custom_action_convention_builder_should_not_implement_introduced_contract()
    {
        // arrange
        var builder = new CustomActionConventionBuilder();

        // act
        var actionBuilder = (IActionConventionBuilder) builder;

        // assert
        actionBuilder.Should().NotBeAssignableTo<IIntroducedInApiVersionConventionBuilder>();
    }

#pragma warning disable IDE0079
#pragma warning disable CA1034 // Nested types should not be visible

    public sealed class CustomActionConventionBuilder : IActionConventionBuilder
    {
        public Type ControllerType => typeof( UndecoratedController );

        public IActionConventionBuilder Action( MethodInfo actionMethod ) => this;

        public void MapToApiVersion( ApiVersion apiVersion ) { }

        public void IsApiVersionNeutral() { }

        public void HasApiVersion( ApiVersion apiVersion ) { }

        public void HasDeprecatedApiVersion( ApiVersion apiVersion ) { }

        public void AdvertisesApiVersion( ApiVersion apiVersion ) { }

        public void AdvertisesDeprecatedApiVersion( ApiVersion apiVersion ) { }

        public void ApplyTo( ActionModel item ) { }
    }

#if !NETFRAMEWORK
    [ApiController]
#endif
    public sealed class UndecoratedController : ControllerBase
    {
        public IActionResult Get() => Ok();
    }

#if !NETFRAMEWORK
    [ApiController]
#endif
    public sealed class DecoratedController : ControllerBase
    {
        public IActionResult Get() => Ok();

        [MapToApiVersion( "2.0" )]
        [MapToApiVersion( "3.0" )]
        public IActionResult GetV2() => Ok();

        [IntroducedInApiVersion( "2.0" )]
        public IActionResult GetIntroduced() => Ok();
    }
}