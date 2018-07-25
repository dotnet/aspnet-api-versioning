namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Options;
    using Xunit;

    public class ApiVersioningRouteOptionsSetupTest
    {
        [Fact]
        public void post_configure_should_add_route_contraint_with_default_name()
        {
            // arrange
            var versioningOptions = Options.Create( new ApiVersioningOptions() );
            var routeOptions = new RouteOptions();
            var setup = new ApiVersioningRouteOptionsSetup( versioningOptions );

            // act
            setup.PostConfigure( default, routeOptions );

            // assert
            routeOptions.ConstraintMap["apiVersion"].Should().Be( typeof( ApiVersionRouteConstraint ) );
        }

        [Fact]
        public void post_configure_should_add_route_contraint_with_custom_name()
        {
            // arrange
            const string key = "api-version";
            var versioningOptions = Options.Create( new ApiVersioningOptions() { RouteConstraintName = key } );
            var routeOptions = new RouteOptions();
            var setup = new ApiVersioningRouteOptionsSetup( versioningOptions );

            // act
            setup.PostConfigure( default, routeOptions );

            // assert
            routeOptions.ConstraintMap[key].Should().Be( typeof( ApiVersionRouteConstraint ) );
        }
    }
}