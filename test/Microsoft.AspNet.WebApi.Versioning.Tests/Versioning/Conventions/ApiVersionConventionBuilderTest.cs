namespace Microsoft.Web.Http.Versioning.Conventions
{
    using FluentAssertions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
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
            conventionBuilder.ProtectedControllerConventionBuilders.Should().BeEquivalentTo(
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
            conventionBuilder.ProtectedControllerConventionBuilders.Should().BeEquivalentTo(
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
            conventionBuilder.ProtectedControllerConventionBuilders.Should().BeEquivalentTo(
                new Dictionary<Type, IApiVersionConvention<HttpControllerDescriptor>>()
                {
                    [typeof( StubController )] = controllerBuilder
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
            var configuration = new HttpConfiguration();
            var controllerDescriptor = new HttpControllerDescriptor( configuration, "Undecorated", typeof( v2.UndecoratedController ) );
            var conventionBuilder = new ApiVersionConventionBuilder();

            conventionBuilder.Add( new VersionByNamespaceConvention() );
            configuration.AddApiVersioning( o => o.Conventions = conventionBuilder );

            var actionDescriptor = configuration.Services.GetActionSelector().GetActionMapping( controllerDescriptor ).SelectMany( g => g ).Single();

            // act
            conventionBuilder.ApplyTo( controllerDescriptor );

            // assert
            actionDescriptor.MappingTo( new ApiVersion( 2, 0 ) ).Should().Be( Implicit );
        }

        sealed class TestApiVersionConventionBuilder : ApiVersionConventionBuilder
        {
            internal IDictionary<Type, IControllerConventionBuilder> ProtectedControllerConventionBuilders => ControllerConventionBuilders;
        }

        sealed class StubController : ApiController
        {
            public IHttpActionResult Get() => Ok();
        }
    }

    namespace v2
    {
        sealed class UndecoratedController : ApiController
        {
            public IHttpActionResult Get() => Ok();
        }
    }
}