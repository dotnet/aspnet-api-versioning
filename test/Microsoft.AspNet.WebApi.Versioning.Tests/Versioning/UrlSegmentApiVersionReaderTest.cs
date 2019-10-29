namespace Microsoft.Web.Http.Versioning
{
    using FluentAssertions;
    using Moq;
    using Routing;
    using System;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using Xunit;
    using static ApiVersionParameterLocation;
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

        [Fact]
        public void add_parameters_should_add_parameter_for_url_segment()
        {
            // arrange
            var reader = new UrlSegmentApiVersionReader();
            var context = new Mock<IApiVersionParameterDescriptionContext>();

            context.Setup( c => c.AddParameter( It.IsAny<string>(), It.IsAny<ApiVersionParameterLocation>() ) );

            // act
            reader.AddParameters( context.Object );

            // assert
            context.Verify( c => c.AddParameter( string.Empty, Path ), Times.Once() );
        }
    }
}