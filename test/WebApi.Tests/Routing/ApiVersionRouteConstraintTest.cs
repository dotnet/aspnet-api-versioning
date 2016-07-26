namespace Microsoft.Web.Http.Routing
{
    using FluentAssertions;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http.Routing;
    using Xunit;
    using static System.Web.Http.Routing.HttpRouteDirection;

    public class ApiVersionRouteConstraintTest
    {
        [Fact]
        public void match_should_return_false_for_uri_generation()
        {
            // arrange
            var request = new HttpRequestMessage();
            var route = new Mock<IHttpRoute>().Object;
            var values = new Dictionary<string, object>();
            var routeDirection = UriGeneration;
            var constraint = new ApiVersionRouteConstraint();

            // act
            var matched = constraint.Match( request, route, "version", values, routeDirection );

            // assert
            matched.Should().BeFalse();
        }

        [Fact]
        public void match_should_return_false_when_route_parameter_is_missing()
        {
            // arrange
            var request = new HttpRequestMessage();
            var route = new Mock<IHttpRoute>().Object;
            var values = new Dictionary<string, object>();
            var routeDirection = UriResolution;
            var constraint = new ApiVersionRouteConstraint();

            // act
            var matched = constraint.Match( request, route, "version", values, routeDirection );

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
            var request = new HttpRequestMessage();
            var route = new Mock<IHttpRoute>().Object;
            var parameterName = nameof( version );
            var values = new Dictionary<string, object>() { [parameterName] = version };
            var routeDirection = UriResolution;
            var constraint = new ApiVersionRouteConstraint();

            // act
            var matched = constraint.Match( request, route, parameterName, values, routeDirection );

            // assert
            matched.Should().BeFalse();
        }

        [Fact]
        public void match_should_return_true_when_matched()
        {
            // arrange
            var request = new HttpRequestMessage();
            var route = new Mock<IHttpRoute>().Object;
            var values = new Dictionary<string, object>() { ["version"] = "2.0" };
            var routeDirection = UriResolution;
            var constraint = new ApiVersionRouteConstraint();

            // act
            var matched = constraint.Match( request, route, "version", values, routeDirection );

            // assert
            matched.Should().BeTrue();
        }
    }
}
