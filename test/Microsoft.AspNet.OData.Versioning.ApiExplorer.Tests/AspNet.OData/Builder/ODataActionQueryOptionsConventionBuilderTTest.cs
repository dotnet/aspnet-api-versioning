namespace Microsoft.AspNet.OData.Builder
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Query;
    using Moq;
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using Xunit;
    using static Microsoft.AspNet.OData.Query.AllowedArithmeticOperators;
    using static Microsoft.AspNet.OData.Query.AllowedFunctions;
    using static Microsoft.AspNet.OData.Query.AllowedLogicalOperators;
    using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;
    using static Moq.Times;

    public class ODataActionQueryOptionsConventionBuilderTTest
    {
        [Fact]
        public void allow_should_enable_arithmetic_operator()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();

            // act
            builder.Allow( Add );

            // assert
            builder.ValidationSettings.AllowedArithmeticOperators.Should().Be( Add );
        }

        [Fact]
        public void allow_should_enable_function()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();

            // act
            builder.Allow( Contains );

            // assert
            builder.ValidationSettings.AllowedFunctions.Should().Be( Contains );
        }

        [Fact]
        public void allow_should_enable_logical_operator()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();

            // act
            builder.Allow( Or | And );

            // assert
            builder.ValidationSettings.AllowedLogicalOperators.Should().Be( Or | And );
        }

        [Fact]
        public void allow_should_enable_query_option()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();

            // act
            builder.Allow( Expand );

            // assert
            builder.ValidationSettings.AllowedQueryOptions.Should().Be( Expand );
        }

        [Fact]
        public void allow_skip_should_enable_query_option_with_max()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();

            // act
            builder.AllowSkip( 42 );

            // assert
            builder.ValidationSettings.Should().BeEquivalentTo(
                new
                {
                    AllowedQueryOptions = Skip,
                    MaxSkip = new int?( 42 ),
                },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void allow_top_should_enable_query_option_with_max()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();

            // act
            builder.AllowTop( 42 );

            // assert
            builder.ValidationSettings.Should().BeEquivalentTo(
                new
                {
                    AllowedQueryOptions = Top,
                    MaxTop = new int?( 42 ),
                },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void allow_expand_should_enable_query_option_with_max_depth()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();

            // act
            builder.AllowExpand( 42 );

            // assert
            builder.ValidationSettings.Should().BeEquivalentTo(
                new
                {
                    AllowedQueryOptions = Expand,
                    MaxExpansionDepth = 42,
                },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void allow_any_or_all_should_enable_query_option_function_and_max_depth()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();

            // act
            builder.AllowAnyAll( 42 );

            // assert
            builder.ValidationSettings.Should().BeEquivalentTo(
                new
                {
                    AllowedFunctions = Any | AllowedFunctions.All,
                    AllowedQueryOptions = Filter,
                    MaxAnyAllExpressionDepth = 42,
                },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void allow_filter_should_enable_query_option_with_max_expressions()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();

            // act
            builder.AllowFilter( 42 );

            // assert
            builder.ValidationSettings.Should().BeEquivalentTo(
                new
                {
                    AllowedQueryOptions = Filter,
                    MaxNodeCount = 42,
                },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void allow_orderby_should_enable_query_option_with_properties_and_max_expressions()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();
            var properties = new[] { "name" }.AsEnumerable();

            // act
            builder.AllowOrderBy( 42, properties );

            // assert
            builder.ValidationSettings.Should().BeEquivalentTo(
                new
                {
                    AllowedQueryOptions = OrderBy,
                    MaxOrderByNodeCount = 42,
                    AllowedOrderByProperties = new[] { "name" },
                },
                options => options.ExcludingMissingMembers() );
        }

        [Fact]
        public void use_should_copy_validation_settings()
        {
            // arrange
            var builder = new TestODataActionQueryOptionsConventionBuilder();
            var settings = new ODataValidationSettings()
            {
                AllowedArithmeticOperators = Add | Subtract,
                AllowedFunctions = Contains | StartsWith | EndsWith,
                AllowedLogicalOperators = Or | And,
                AllowedOrderByProperties = { "firstName", "lastName" },
                AllowedQueryOptions = Select | Expand | OrderBy | Filter,
                MaxAnyAllExpressionDepth = 3,
                MaxExpansionDepth = 3,
                MaxNodeCount = 10,
                MaxOrderByNodeCount = 3,
                MaxTop = 100,
            };

            // act
            builder.Use( settings );

            // assert
            builder.ValidationSettings.Should().BeEquivalentTo( settings );
        }

        [Fact]
        public void action_should_call_action_on_controller_builder()
        {
            // arrange
            var controllerBuilder = new Mock<ODataControllerQueryOptionsConventionBuilder<TestController>>();
            var actionBuilder = new ODataActionQueryOptionsConventionBuilder<TestController>( controllerBuilder.Object );
            var method = typeof( TestController ).GetMethod( nameof( TestController.Get ) );

            controllerBuilder.Setup( cb => cb.Action( It.IsAny<MethodInfo>() ) );

            // act
            actionBuilder.Action( method );

            // assert
            controllerBuilder.Verify( cb => cb.Action( method ), Once() );
        }

        public sealed class TestController : ApiController
        {
            public IHttpActionResult Get() => Ok();
        }

        sealed class TestODataActionQueryOptionsConventionBuilder : ODataActionQueryOptionsConventionBuilder<IHttpController>
        {
            internal TestODataActionQueryOptionsConventionBuilder()
                : base( new ODataControllerQueryOptionsConventionBuilder<IHttpController>() ) { }

            internal new ODataValidationSettings ValidationSettings => base.ValidationSettings;
        }
    }
}