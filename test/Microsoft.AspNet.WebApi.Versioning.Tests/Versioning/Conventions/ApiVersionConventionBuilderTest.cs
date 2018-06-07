namespace Microsoft.Web.Http.Versioning.Conventions
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Web.Http;
    using System.Web.Http.Controllers;
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
            conventionBuilder.ProtectedControllerConventions.Should().BeEquivalentTo(
                new Dictionary<Type, IApiVersionConvention<HttpControllerDescriptor>>()
                {
                    [typeof( StubController )] = controllerBuilder
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
            conventionBuilder.ProtectedControllerConventions.Should().BeEquivalentTo(
                new Dictionary<Type, IApiVersionConvention<HttpControllerDescriptor>>()
                {
                    [typeof( StubController )] = controllerBuilder
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
            conventionBuilder.ProtectedControllerConventions.Should().BeEquivalentTo(
                new Dictionary<Type, IApiVersionConvention<HttpControllerDescriptor>>()
                {
                    [typeof( StubController )] = controllerBuilder
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
            conventionBuilder.ProtectedControllerConventions.Should().BeEquivalentTo(
                new Dictionary<Type, IApiVersionConvention<HttpControllerDescriptor>>()
                {
                    [typeof( StubController )] = controllerBuilder
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
            controllerConvention.Should().Throw<InvalidOperationException>();
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
            controllerConvention.Should().Throw<InvalidOperationException>();
        }

        sealed class TestApiVersionConventionBuilder : ApiVersionConventionBuilder
        {
            internal IDictionary<Type, IApiVersionConvention<HttpControllerDescriptor>> ProtectedControllerConventions => ControllerConventions;
        }

        sealed class StubController : ApiController
        {
            public IHttpActionResult Get() => Ok();
        }
    }
}