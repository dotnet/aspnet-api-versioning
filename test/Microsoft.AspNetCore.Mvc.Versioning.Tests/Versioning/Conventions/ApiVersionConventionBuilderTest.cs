namespace Microsoft.AspNetCore.Mvc.Versioning.Conventions
{
    using ApplicationModels;
    using FluentAssertions;
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;

    public class ApiVersionConventionBuilderTest
    {
        private sealed class TestApiVersionConventionBuilder : ApiVersionConventionBuilder
        {
            internal IDictionary<TypeInfo, IApiVersionConvention<ControllerModel>> ProtectedControllerConventions => ControllerConventions;
        }

        private sealed class StubController : Controller
        {
            public IActionResult Get() => Ok();
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
    }
}