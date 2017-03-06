namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using AspNetCore.Routing;
    using FluentAssertions;
    using Http;
    using Http.Features;
    using Moq;
    using System.Collections.Generic;
    using Xunit;

    public class UrlSegmentApiVersionReaderTest
    {
        [Fact]
        public void read_should_retrieve_version_from_url()
        {
            // arrange
            var requestedVersion = "2";
            var request = RequestAfterApiVersionConstraintHasBeenMatched( requestedVersion );
            var reader = new UrlSegmentApiVersionReader();

            // act
            var version = reader.Read( request );

            // assert
            version.Should().Be( requestedVersion );
        }

        static HttpRequest RequestAfterApiVersionConstraintHasBeenMatched( string requestedVersion )
        {
            const string ParmaterName = "version";
            const string ItemKey = "MS_ApiVersionRequestProperties";
            var request = new Mock<HttpRequest>();

            var routeData = new RouteData() { Values = { [ParmaterName] = requestedVersion } };
            var feature = new RoutingFeature() { RouteData = routeData };
            var featureCollection = new Mock<IFeatureCollection>();
            var items = new Dictionary<object, object>();
            var httpContext = new Mock<HttpContext>();
            var reader = new UrlSegmentApiVersionReader();

            featureCollection.SetupGet( fc => fc[typeof( IRoutingFeature )] ).Returns( feature );
            httpContext.SetupProperty( c => c.Items, items );
            httpContext.SetupGet( c => c.Features ).Returns( featureCollection.Object );

            var properties = new ApiVersionRequestProperties( httpContext.Object ) { RouteParameterName = ParmaterName };

            items[ItemKey] = properties;
            request.SetupGet( r => r.HttpContext ).Returns( httpContext.Object );

            return request.Object;
        }
    }
}