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
        sealed class TestApiVersionConventionBuilder : ApiVersionConventionBuilder
        {
            internal IDictionary<Type, IApiVersionConvention<HttpControllerDescriptor>> ProtectedControllerConventions => ControllerConventions;
        }

        sealed class StubController : ApiController
        {
            public IHttpActionResult Get() => Ok();
        }

        [Fact]
        public void controller_should_add_new_controller_builder_conventions()
        {
            // arrange
            var conventionBuilder = new TestApiVersionConventionBuilder();

            // act
            var controllerBuilder = conventionBuilder.Controller<StubController>();

            // assert
            conventionBuilder.ProtectedControllerConventions.ShouldBeEquivalentTo(
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
            conventionBuilder.ProtectedControllerConventions.ShouldBeEquivalentTo(
                new Dictionary<Type, IApiVersionConvention<HttpControllerDescriptor>>()
                {
                    [typeof( StubController )] = controllerBuilder
                } );
        }
    }
}