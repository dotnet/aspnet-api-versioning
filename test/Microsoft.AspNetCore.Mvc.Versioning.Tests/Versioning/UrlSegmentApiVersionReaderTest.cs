namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using AspNetCore.Routing;
    using FluentAssertions;
    using Http;
    using Http.Features;
    using Microsoft.AspNetCore.Mvc.Routing;
    using Moq;
    using System;
    using Xunit;
    using static AspNetCore.Routing.RouteDirection;

    public class UrlSegmentApiVersionReaderTest
    {
        [Fact]
        public void read_should_retrieve_version_from_url()
        {
            // arrange
            var requestedVersion = "2";
            var constraint = new ApiVersionRouteConstraint();
            var reader = new UrlSegmentApiVersionReader();
            var request = RequestAfterApiVersionConstraintHasBeenMatched( requestedVersion, reader );
            var httpContext = request.HttpContext;
            var route = default( IRouter );
            var routeData = ( (RoutingFeature) httpContext.Features[typeof( IRoutingFeature )] ).RouteData;

            constraint.Match( httpContext, route, "version", routeData.Values, IncomingRequest );

            // act
            var version = reader.Read( request );

            // assert
            version.Should().Be( requestedVersion );
        }

        static HttpRequest RequestAfterApiVersionConstraintHasBeenMatched( string requestedVersion, IApiVersionReader apiVersionReader )
        {
            const string ParmaterName = "version";

            var request = new Mock<HttpRequest>();
            var routeData = new RouteData() { Values = { [ParmaterName] = requestedVersion } };
            var featureCollection = new FeatureCollection();
            var requestServices = new Mock<IServiceProvider>();
            var httpContext = new Mock<HttpContext>();

            requestServices.Setup( rs => rs.GetService( typeof( IApiVersionReader ) ) ).Returns( apiVersionReader );
            httpContext.SetupGet( c => c.Features ).Returns( featureCollection );
            httpContext.SetupProperty( c => c.RequestServices, requestServices.Object );
            httpContext.SetupGet( c => c.Request ).Returns( () => request.Object );
            request.SetupGet( r => r.HttpContext ).Returns( httpContext.Object );
            featureCollection.Set<IApiVersioningFeature>( new ApiVersioningFeature( httpContext.Object ) );
            featureCollection.Set<IRoutingFeature>( new RoutingFeature() { RouteData = routeData } );

            return request.Object;
        }
    }
}