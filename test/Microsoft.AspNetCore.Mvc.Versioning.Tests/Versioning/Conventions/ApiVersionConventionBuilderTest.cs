namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using ApplicationModels;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;
    using static ApiVersionMapping;

    public class ApiVersionConventionBuilderTest
    {
        [Fact]
        public void controller_should_add_new_controller_builder_conventions()
        {
            // arrange
            var conventionBuilder = new TestApiVersionConventionBuilder();

            // act
            var controllerBuilder = conventionBuilder.Controller<StubController>();

            // assert
            conventionBuilder.ProtectedControllerConventionBuilders.Should().BeEquivalentTo(
                new Dictionary<TypeInfo, IApiVersionConvention<ControllerModel>>()
                {
                    [typeof( StubController ).GetTypeInfo()] = controllerBuilder
                } );
        }

        [Fact]
        public void controller_should_add_new_controller_builder_conventions_for_type()
        {
            // arrange
            var conventionBuilder = new TestApiVersionConventionBuilder();

            // act
            var controllerBuilder = conventionBuilder.Controller( typeof( StubController ) );

            // assert
            conventionBuilder.ProtectedControllerConventionBuilders.Should().BeEquivalentTo(
                new Dictionary<TypeInfo, IApiVersionConvention<ControllerModel>>()
                {
                    [typeof( StubController ).GetTypeInfo()] = controllerBuilder
                } );
        }

        [Fact]
        public void controller_should_return_existing_controller_builder_conventions()
        {
            // arrange
            var conventionBuilder = new TestApiVersionConventionBuilder();
            var originalControllerBuilder = conventionBuilder.Controller<StubController>();

            // act
            var controllerBuilder = conventionBuilder.Controller<StubController>();

            // assert
            controllerBuilder.Should().BeSameAs( originalControllerBuilder );
            conventionBuilder.ProtectedControllerConventionBuilders.Should().BeEquivalentTo(
                new Dictionary<TypeInfo, IApiVersionConvention<ControllerModel>>()
                {
                    [typeof( StubController ).GetTypeInfo()] = controllerBuilder
                } );
        }

        [Fact]
        public void controller_should_return_existing_controller_builder_conventions_for_type()
        {
            // arrange
            var conventionBuilder = new TestApiVersionConventionBuilder();
            var originalControllerBuilder = conventionBuilder.Controller( typeof( StubController ) );

            // act
            var controllerBuilder = conventionBuilder.Controller( typeof( StubController ) );

            // assert
            controllerBuilder.Should().BeSameAs( originalControllerBuilder );
            conventionBuilder.ProtectedControllerConventionBuilders.Should().BeEquivalentTo(
                new Dictionary<TypeInfo, IApiVersionConvention<ControllerModel>>()
                {
                    [typeof( StubController ).GetTypeInfo()] = controllerBuilder
                } );
        }

        [Fact]
        public void controller_should_allow_both_compileX2Dtime_and_runX2Dtime_conventions()
        {
            // arrange
            var conventionBuilder = new ApiVersionConventionBuilder();

            conventionBuilder.Controller<StubController>();

            // act
            Action controllerConvention = () => conventionBuilder.Controller( typeof( StubController ) );

            // assert
            controllerConvention.Should().NotThrow<InvalidOperationException>();
        }

        [Fact]
        public void controller_for_type_should_allow_both_compileX2Dtime_and_runX2Dtime_conventions()
        {
            // arrange
            var conventionBuilder = new ApiVersionConventionBuilder();

            conventionBuilder.Controller( typeof( StubController ) );

            // act
            Action controllerConvention = () => conventionBuilder.Controller<StubController>();

            // assert
            controllerConvention.Should().NotThrow<InvalidOperationException>();
        }

        [Fact]
        public void apply_should_apply_configured_conventions()
        {
            // arrange
            var controllerType = typeof( v2.UndecoratedController ).GetTypeInfo();
            var action = controllerType.GetRuntimeMethod( nameof( v2.UndecoratedController.Get ), Type.EmptyTypes );
            var attributes = Array.Empty<object>();
            var actionModel = new ActionModel( action, attributes );
            var controllerModel = new ControllerModel( controllerType, attributes ) { Actions = { actionModel } };
            var conventionBuilder = new ApiVersionConventionBuilder();
            var actionDescriptor = new ActionDescriptor();

            conventionBuilder.Add( new VersionByNamespaceConvention() );

            // act
            conventionBuilder.ApplyTo( controllerModel );
            actionDescriptor.SetProperty( controllerModel );
            actionDescriptor.SetProperty( actionModel.GetProperty<ApiVersionModel>() );

            // assert
            actionDescriptor.MappingTo( new ApiVersion( 2, 0 ) ).Should().Be( Implicit );
        }

        sealed class TestApiVersionConventionBuilder : ApiVersionConventionBuilder
        {
            internal IDictionary<TypeInfo, IControllerConventionBuilder> ProtectedControllerConventionBuilders => ControllerConventionBuilders;
        }

        [ApiController]
        sealed class StubController : ControllerBase
        {
            public IActionResult Get() => Ok();
        }
    }

    namespace v2
    {
        [ApiController]
        sealed class UndecoratedController : ControllerBase
        {
            public IActionResult Get() => Ok();
        }
    }
}