namespace Microsoft.AspNetCore.Mvc.Routing
{
    using AspNetCore.Routing;
    using FluentAssertions;
    using Http;
    using Moq;
    using System.Collections.Generic;
    using Xunit;
    using static AspNetCore.Routing.RouteDirection;

    public class ApiVersionRouteConstraintTest
    {
        [Fact]
        public void match_should_return_false_for_url_generation()
        {
            // arrange
            var httpContext = new Mock<HttpContext>().Object;
            var route = new Mock<IRouter>().Object;
            var values = new RouteValueDictionary();
            var routeDirection = UrlGeneration;
            var constraint = new ApiVersionRouteConstraint();

            // act
            var matched = constraint.Match( httpContext, route, null, values, routeDirection );

            // assert
            matched.Should().BeFalse();
        }

        [Fact]
        public void match_should_return_false_when_route_key_is_missing()
        {
            // arrange
            var httpContext = new Mock<HttpContext>();
            var route = new Mock<IRouter>().Object;
            var values = new RouteValueDictionary();
            var routeDirection = IncomingRequest;
            var constraint = new ApiVersionRouteConstraint();

            httpContext.SetupProperty( c => c.Items, new Dictionary<object, object>() );

            // act
            var matched = constraint.Match( httpContext.Object, route, "version", values, routeDirection );

            // assert
            matched.Should().BeFalse();
        }

        [Theory]
        [InlineData( null )]
        [InlineData( "" )]
        [InlineData( "abc" )]
        public void match_should_return_false_when_route_parameter_is_invalid( string version )
        {
            // arrange
            var httpContext = new Mock<HttpContext>();
            var route = new Mock<IRouter>().Object;
            var routeKey = nameof( version );
            var values = new RouteValueDictionary() { [routeKey] = version };
            var routeDirection = IncomingRequest;
            var constraint = new ApiVersionRouteConstraint();

            httpContext.SetupProperty( c => c.Items, new Dictionary<object, object>() );

            // act
            var matched = constraint.Match( httpContext.Object, route, routeKey, values, routeDirection );

            // assert
            matched.Should().BeFalse();
        }

        [Fact]
        public void match_should_return_true_when_matched()
        {
            // arrange
            var httpContext = new Mock<HttpContext>();
            var route = new Mock<IRouter>().Object;
            var values = new RouteValueDictionary() { ["version"] = "2.0" };
            var routeDirection = IncomingRequest;
            var constraint = new ApiVersionRouteConstraint();

            httpContext.SetupProperty( c => c.Items, new Dictionary<object, object>() );

            // act
            var matched = constraint.Match( httpContext.Object, route, "version", values, routeDirection );

            // assert
            matched.Should().BeTrue();
        }
    }
}
