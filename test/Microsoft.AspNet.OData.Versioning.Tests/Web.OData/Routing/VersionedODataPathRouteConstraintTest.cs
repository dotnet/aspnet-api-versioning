namespace Microsoft.Web.OData.Routing
{
    using FluentAssertions;
    using Http;
    using Microsoft.OData.Edm;
    using Moq;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using System.Web.OData.Builder;
    using System.Web.OData.Routing;
    using System.Web.OData.Routing.Conventions;
    using Xunit;
    using static Http.ApiVersion;
    using static System.Net.Http.HttpMethod;
    using static System.Web.Http.Routing.HttpRouteDirection;

    public class VersionedODataPathRouteConstraintTest
    {
        public class Test
        {
            public int Id { get; set; }
        }

        private static IEdmModel EmptyModel => new ODataModelBuilder().GetEdmModel();

        private static IEdmModel TestModel
        {
            get
            {
                var builder = new ODataModelBuilder();
                var tests = builder.EntitySet<Test>( "Tests" ).EntityType;
                tests.HasKey( t => t.Id );
                return builder.GetEdmModel();
            }
        }

        private static VersionedODataPathRouteConstraint NewVersionedODataPathRouteConstraint( HttpRequestMessage request, IEdmModel model, ApiVersion apiVersion, string routePrefix = null )
        {
            var pathHandler = new DefaultODataPathHandler();
            var conventions = ODataRoutingConventions.CreateDefault();
            var configuration = new HttpConfiguration();
            var routingConventions = Enumerable.Empty<IODataRoutingConvention>();
            var constraint = new VersionedODataPathRouteConstraint( pathHandler, model, "odata", routingConventions, apiVersion );

            configuration.AddApiVersioning();
            configuration.MapVersionedODataRoute( "odata", routePrefix, model, apiVersion );
            request.SetConfiguration( configuration );

            return constraint;
        }

        [Fact]
        public void match_should_only_evaluate_uri_resolution()
        {
            // arrange
            var request = new HttpRequestMessage();
            var route = new Mock<IHttpRoute>().Object;
            var parameterName = (string) null;
            var values = new Dictionary<string, object>();
            var routeDirection = UriGeneration;
            var pathHandler = new Mock<IODataPathHandler>().Object;
            var model = new Mock<IEdmModel>().Object;
            var routingConventions = Enumerable.Empty<IODataRoutingConvention>();
            var constraint = new VersionedODataPathRouteConstraint( pathHandler, model, "odata", routingConventions, Default );

            // act
            var result = constraint.Match( request, route, parameterName, values, routeDirection );

            // assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData( "2.0" )]
        [InlineData( "3.0" )]
        public void match_should_be_true_when_api_version_is_requested_in_query_string( string apiVersion )
        {
            // arrange
            var model = TestModel;
            var request = new HttpRequestMessage( Get, $"http://localhost/Tests(1)?api-version={apiVersion}" );
            var values = new Dictionary<string, object>() { { "odataPath", "Tests(1)" } };
            var constraint = NewVersionedODataPathRouteConstraint( request, model, Parse( apiVersion ) );

            // act
            var result = constraint.Match( request, null, null, values, UriResolution );

            // assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData( "http://localhost", null, "1.0", true )]
        [InlineData( "http://localhost", null, "2.0", false )]
        [InlineData( "http://localhost/$metadata", "$metadata", "1.0", true )]
        [InlineData( "http://localhost/$metadata", "$metadata", "2.0", false )]
        public void match_should_return_expected_result_for_service_and_metadata_document( string requestUri, string odataPath, string apiVersionValue, bool expected )
        {
            // arrange
            var apiVersion = Parse( apiVersionValue );
            var model = EmptyModel;
            var request = new HttpRequestMessage( Get, requestUri );
            var values = new Dictionary<string, object>() { { "odataPath", odataPath } };
            var constraint = NewVersionedODataPathRouteConstraint( request, model, apiVersion );

            // act
            var result = constraint.Match( request, null, null, values, UriResolution );

            // assert
            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( true, true )]
        [InlineData( false, false )]
        public void match_should_return_expected_result_when_controller_is_implicitly_versioned( bool allowImplicitVersioning, bool expected )
        {
            // arrange
            var apiVersion = new ApiVersion( 2, 0 );
            var model = TestModel;
            var request = new HttpRequestMessage( Get, $"http://localhost/Tests(1)" );
            var values = new Dictionary<string, object>() { { "odataPath", "Tests(1)" } };
            var constraint = NewVersionedODataPathRouteConstraint( request, model, apiVersion );

            request.GetConfiguration().AddApiVersioning(
                o =>
                {
                    o.DefaultApiVersion = apiVersion;
                    o.AssumeDefaultVersionWhenUnspecified = allowImplicitVersioning;
                } );

            // act
            var result = constraint.Match( request, null, null, values, UriResolution );

            // assert
            result.Should().Be( expected );
        }

        [Theory]
        [InlineData( "v1", "1.0" )]
        [InlineData( "v2.0", "2.0" )]
        public void match_should_set_requested_api_version_from_route_prefix( string routePrefix, string apiVersion )
        {
            // arrange
            var requestedVersion = Parse( apiVersion );
            var model = TestModel;
            var request = new HttpRequestMessage( Get, $"http://localhost/{routePrefix}/Tests(1)" );
            var values = new Dictionary<string, object>() { { "odataPath", "Tests(1)" } };
            var constraint = NewVersionedODataPathRouteConstraint( request, model, requestedVersion, routePrefix );
            var route = request.GetConfiguration().Routes.Single();

            // act
            var result = constraint.Match( request, route, null, values, UriResolution );

            // assert
            result.Should().BeTrue();
            request.GetRequestedApiVersion().Should().Be( requestedVersion );
        }
    }
}
