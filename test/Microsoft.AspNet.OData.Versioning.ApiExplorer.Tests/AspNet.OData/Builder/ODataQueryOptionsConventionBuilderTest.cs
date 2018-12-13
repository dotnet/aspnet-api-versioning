namespace Microsoft.AspNet.OData.Builder
{
    using FluentAssertions;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http.Description;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Description;
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
                    [typeof( StubController )] = controllerBuilder
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
            var controller = new HttpControllerDescriptor( new HttpConfiguration(), "Stub", typeof( StubController ) );
            var action = new ReflectedHttpActionDescriptor( controller, typeof( StubController ).GetMethod( nameof( StubController.Get ) ) );
            var description = new VersionedApiDescription()
            {
                ActionDescriptor = action,
                HttpMethod = HttpMethod.Get,
                ResponseDescription = new ResponseDescription()
                {
                    ResponseType = typeof( object ),
                },
                Properties =
                {
                    [typeof( IEdmModel )] = new EdmModel(),
                }
            };
            var builder = new ODataQueryOptionsConventionBuilder();
            var settings = new ODataQueryOptionSettings() { DescriptionProvider = builder.DescriptionProvider };
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
            internal new IDictionary<Type, IODataQueryOptionsConventionBuilder> ConventionBuilders => base.ConventionBuilders;

            internal new IList<IODataQueryOptionsConvention> Conventions => base.Conventions;
        }

        public sealed class StubController : ODataController
        {
            public IHttpActionResult Get() => Ok();
        }
    }
}