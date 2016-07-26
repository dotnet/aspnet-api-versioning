namespace System.Web.OData
{
    using FluentAssertions;
    using Batch;
    using Builder;
    using Collections.Generic;
    using Collections.ObjectModel;
    using Http;
    using Http.Controllers;
    using Http.Dispatcher;
    using Linq;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.OData.Builder;
    using Microsoft.Web.OData.Routing;
    using Moq;
    using Routing.Conventions;
    using Xunit;

    public class HttpConfigurationExtensionsTest
    {
        private static IEnumerable<IEdmModel> CreateModels( HttpConfiguration configuration )
        {
            var controllerDescriptor = new Mock<HttpControllerDescriptor>( configuration, "Test", typeof( IHttpController ) ) { CallBase = true };
            var controllerMapping = new Dictionary<string, HttpControllerDescriptor>() { { "Test", controllerDescriptor.Object } };
            var controllerSelector = new Mock<IHttpControllerSelector>();
            var apiVersions = new Collection<ApiVersionAttribute>( new[] { new ApiVersionAttribute( "1.0" ), new ApiVersionAttribute( "2.0" ) } );
            var builder = new VersionedODataModelBuilder( controllerSelector.Object );

            controllerDescriptor.Setup( cd => cd.GetCustomAttributes<ApiVersionAttribute>( It.IsAny<bool>() ) ).Returns( apiVersions );
            controllerSelector.Setup( cs => cs.GetControllerMapping() ).Returns( controllerMapping );

            return builder.GetEdmModels();
        }

        [Fact]
        public void map_versioned_odata_routes_should_return_expected_result()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );
            var routeName = "odata";
            var routePrefix = "api/v3";
            var model = new ODataModelBuilder().GetEdmModel();
            var apiVersion = new ApiVersion( 3, 0 );
            var batchHandler = new DefaultODataBatchHandler( httpServer );

            // act
            var route = configuration.MapVersionedODataRoute( routeName, routePrefix, model, apiVersion, batchHandler );
            var constraint = (VersionedODataPathRouteConstraint) route.PathRouteConstraint;
            var batchRoute = configuration.Routes["odataBatch"];

            // assert
            constraint.RoutingConventions[0].Should().BeOfType<AttributeRoutingConvention>();
            constraint.RoutingConventions[1].Should().BeOfType<VersionedMetadataRoutingConvention>();
            constraint.RoutingConventions.OfType<MetadataRoutingConvention>().Should().BeEmpty();
            constraint.RouteName.Should().Be( routeName );
            route.RoutePrefix.Should().Be( routePrefix );
            batchRoute.Handler.Should().Be( batchHandler );
            batchRoute.RouteTemplate.Should().Be( "api/v3/$batch" );
        }

        [Fact]
        public void map_versioned_odata_routes_should_return_expected_results()
        {
            // arrange
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );
            var routeName = "odata";
            var routePrefix = "api";
            var batchHandler = new DefaultODataBatchHandler( httpServer );
            var models = CreateModels( configuration );

            // act
            var routes = configuration.MapVersionedODataRoutes( routeName, routePrefix, models, batchHandler );
            var batchRoute = configuration.Routes["odataBatch"];

            // assert
            foreach ( var route in routes )
            {
                var constraint = (VersionedODataPathRouteConstraint) route.PathRouteConstraint;
                var apiVersion = constraint.EdmModel.GetAnnotationValue<ApiVersionAnnotation>( constraint.EdmModel ).ApiVersion;
                var versionedRouteName = routeName + "-" + apiVersion.ToString();

                constraint.RoutingConventions[0].Should().BeOfType<AttributeRoutingConvention>();
                constraint.RoutingConventions[1].Should().BeOfType<VersionedMetadataRoutingConvention>();
                constraint.RoutingConventions.OfType<MetadataRoutingConvention>().Should().BeEmpty();
                constraint.RouteName.Should().Be( versionedRouteName );
                route.RoutePrefix.Should().Be( routePrefix );
            }

            batchRoute.Handler.Should().Be( batchHandler );
            batchRoute.RouteTemplate.Should().Be( "api/$batch" );
        }
    }
}
