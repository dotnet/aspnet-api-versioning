namespace Microsoft.Web.Http.Versioning.Conventions
{
    using FluentAssertions;
    using Moq;
    using System;
    using System.Web.Http;
    using Xunit;
    using static Moq.Times;

    public class ActionConventionBuilderExtensionsTest
    {
        public sealed class StubController : ApiController
        {
            public IHttpActionResult Get() => Ok();

            public void Delete() {  }

            public TimeSpan Timeout { get; set; }
        }

        [Fact]
        public void action_should_map_method_from_action_delegate_expression()
        {
            // arrange
            var method = typeof( StubController ).GetMethod( nameof( StubController.Delete ) );
            var builder = new Mock<IActionConventionBuilder<StubController>>();

            // act
            builder.Object.Action( c => c.Delete() );

            // assert
            builder.Verify( b => b.Action( method ), Once() );
        }

        [Fact]
        public void action_should_map_method_from_func_delegate_expression()
        {
            // arrange
            var method = typeof( StubController ).GetMethod( nameof( StubController.Get ) );
            var builder = new Mock<IActionConventionBuilder<StubController>>();

            // act
            builder.Object.Action( c => c.Get() );

            // assert
            builder.Verify( b => b.Action( method ), Once() );
        }

        [Fact]
        public void action_should_throw_exception_when_func_delegate_expression_is_not_a_method()
        {
            // arrange
            var builder = new Mock<IActionConventionBuilder<StubController>>().Object;

            // act
            Action action = () => builder.Action( c => c.Timeout );

            // assert
            action.ShouldThrow<InvalidOperationException>().And
                  .Message.Should().Be( "The expression 'c => c.Timeout' must refer to a controller action method." );
        }
    }
}
