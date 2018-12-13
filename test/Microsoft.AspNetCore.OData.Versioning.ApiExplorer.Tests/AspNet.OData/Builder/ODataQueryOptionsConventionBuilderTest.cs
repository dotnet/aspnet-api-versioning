namespace Microsoft.AspNet.OData.Builder
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApiExplorer;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Xunit;
    using static Moq.Times;

    public class ODataQueryOptionsConventionBuilderTest
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
                new Dictionary<TypeInfo, IODataQueryOptionsConventionBuilder>()
                {
                    [typeof( StubController ).GetTypeInfo()] = controllerBuilder,
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
                new Dictionary<TypeInfo, IODataQueryOptionsConventionBuilder>()
                {
                    [typeof( StubController ).GetTypeInfo()] = controllerBuilder
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

        [Fact]
        public void apply_should_apply_configured_conventions()
        {
            // arrange
            var description = new ApiDescription()
            {
                ActionDescriptor = new ControllerActionDescriptor()
                {
                    ControllerTypeInfo = typeof( StubController ).GetTypeInfo(),
                    MethodInfo = typeof( StubController ).GetTypeInfo().GetRuntimeMethod( nameof( StubController.Get ), Type.EmptyTypes ),
                },
                HttpMethod = "GET",
            };
            var builder = new ODataQueryOptionsConventionBuilder();
            var settings = new ODataQueryOptionSettings()
            {
                DescriptionProvider = builder.DescriptionProvider,
                DefaultQuerySettings = new DefaultQuerySettings(),
                ModelMetadataProvider = Mock.Of<IModelMetadataProvider>(),
            };
            var convention = new Mock<IODataQueryOptionsConvention>();

            convention.Setup( c => c.ApplyTo( It.IsAny<ApiDescription>() ) );
            builder.Add( convention.Object );

            // act
            builder.ApplyTo( new[] { description }, settings );

            // assert
            convention.Verify( c => c.ApplyTo( description ), Once() );
        }

        sealed class TestODataQueryOptionsConventionBuilder : ODataQueryOptionsConventionBuilder
        {
            internal new IDictionary<TypeInfo, IODataQueryOptionsConventionBuilder> ConventionBuilders => base.ConventionBuilders;

            internal new IList<IODataQueryOptionsConvention> Conventions => base.Conventions;
        }

        public sealed class StubController : ODataController
        {
            public IActionResult Get() => Ok();
        }
    }
}