namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using ApplicationModels;
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;

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
            conventionBuilder.ProtectedControllerConventions.ShouldBeEquivalentTo(
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
            conventionBuilder.ProtectedControllerConventions.ShouldBeEquivalentTo(
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
            conventionBuilder.ProtectedControllerConventions.ShouldBeEquivalentTo(
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
            conventionBuilder.ProtectedControllerConventions.ShouldBeEquivalentTo(
                new Dictionary<TypeInfo, IApiVersionConvention<ControllerModel>>()
                {
                    [typeof( StubController ).GetTypeInfo()] = controllerBuilder
                } );
        }

        [Fact]
        public void controller_should_not_allow_both_compileX2Dtime_and_runX2Dtime_conventions()
        {
            // arrange
            var conventionBuilder = new ApiVersionConventionBuilder();

            conventionBuilder.Controller<StubController>();

            // act
            Action controllerConvention = () => conventionBuilder.Controller( typeof( StubController ) );

            // assert
            controllerConvention.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void controller_for_type_should_not_allow_both_compileX2Dtime_and_runX2Dtime_conventions()
        {
            // arrange
            var conventionBuilder = new ApiVersionConventionBuilder();

            conventionBuilder.Controller( typeof( StubController ) );

            // act
            Action controllerConvention = () => conventionBuilder.Controller<StubController>();

            // assert
            controllerConvention.ShouldThrow<InvalidOperationException>();
        }

        sealed class TestApiVersionConventionBuilder : ApiVersionConventionBuilder
        {
            internal IDictionary<TypeInfo, IApiVersionConvention<ControllerModel>> ProtectedControllerConventions => ControllerConventions;
        }

        sealed class StubController : Controller
        {
            public IActionResult Get() => Ok();
        }
    }
}