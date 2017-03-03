namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using Routing;
    using System;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using Xunit;
    using static System.Net.Http.HttpMethod;

    public class UrlSegmentApiVersionReaderTest
    {
        [Fact]
        public void read_should_retrieve_version_from_url()
        {
            // arrange
            var requestedVersion = "2";
            var configuration = NewConfiguration();
            var request = new HttpRequestMessage( Get, $"http://localhost/api/v{requestedVersion}/test" );
            var reader = new UrlSegmentApiVersionReader();

            configuration.EnsureInitialized();

            var routeData = configuration.Routes.GetRouteData( request );

            request.SetConfiguration( configuration );
            request.SetRouteData( routeData );

            // act
            var version = reader.Read( request );

            // assert
            version.Should().Be( requestedVersion );
        }

        static HttpConfiguration NewConfiguration()
        {
            var configuration = new HttpConfiguration();
            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) }
            };

            configuration.MapHttpAttributeRoutes( constraintResolver );

            return configuration;
        }
    }
}