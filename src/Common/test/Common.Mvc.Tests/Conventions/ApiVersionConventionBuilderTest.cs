// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Dtime

namespace Asp.Versioning.Conventions
{
#if NETFRAMEWORK
    using System.Web.Http;
    using System.Web.Http.Results;
    using ControllerBase = System.Web.Http.ApiController;
    using ControllerModel = System.Web.Http.Controllers.HttpControllerDescriptor;
#else
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
#endif

    public partial class ApiVersionConventionBuilderTest
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
                new Dictionary<Type, IApiVersionConvention<ControllerModel>>()
                {
                    [typeof( StubController )] = controllerBuilder,
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
                new Dictionary<Type, IApiVersionConvention<ControllerModel>>()
                {
                    [typeof( StubController )] = controllerBuilder,
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
                new Dictionary<Type, IApiVersionConvention<ControllerModel>>()
                {
                    [typeof( StubController )] = controllerBuilder,
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
                new Dictionary<Type, IApiVersionConvention<ControllerModel>>()
                {
                    [typeof( StubController )] = controllerBuilder,
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

        private sealed class TestApiVersionConventionBuilder : ApiVersionConventionBuilder
        {
            internal IDictionary<Type, IControllerConventionBuilder> ProtectedControllerConventionBuilders => ControllerConventionBuilders;
        }

#pragma warning disable IDE0079
#pragma warning disable CA1812

        private sealed class StubController : ControllerBase
        {
            public OkResult Get() => Ok();
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
#pragma warning disable SA1403 // File may only contain a single namespace
    namespace v2
    {
#if !NETFRAMEWORK
        [ApiController]
#endif
        internal sealed class UndecoratedController : ControllerBase
        {
            public OkResult Get() => Ok();
        }
    }
}