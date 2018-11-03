namespace Microsoft.AspNet.OData
{
    using FluentAssertions;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Moq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Routing;
    using Xunit;

    public class HttpConfigurationExtensionsTest
    {
        [Fact]
        public void map_versioned_odata_route_should_return_expected_result()
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
            var constraint = route.PathRouteConstraint;
            var routingConventions = GetRoutingConventions( configuration, route );
            var batchRoute = configuration.Routes["odataBatch"];

            // assert
            routingConventions[0].Should().BeOfType<VersionedAttributeRoutingConvention>();
            routingConventions[1].Should().BeOfType<VersionedMetadataRoutingConvention>();
            routingConventions.OfType<MetadataRoutingConvention>().Should().BeEmpty();
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
                if ( !( route.PathRouteConstraint is VersionedODataPathRouteConstraint constraint ) )
                {
                    continue;
                }

                var apiVersion = constraint.ApiVersion;
                var routingConventions = GetRoutingConventions( configuration, route );
                var versionedRouteName = routeName + "-" + apiVersion.ToString();

                routingConventions[0].Should().BeOfType<VersionedAttributeRoutingConvention>();
                routingConventions[1].Should().BeOfType<VersionedMetadataRoutingConvention>();
                routingConventions.OfType<MetadataRoutingConvention>().Should().BeEmpty();
                constraint.RouteName.Should().Be( versionedRouteName );
                route.RoutePrefix.Should().Be( routePrefix );
            }

            batchRoute.Handler.Should().Be( batchHandler );
            batchRoute.RouteTemplate.Should().Be( "api/$batch" );
        }

        [Theory]
        [InlineData( 0, "1.0" )]
        [InlineData( 1, "2.0" )]
        public void get_edm_model_should_retrieve_configured_model_by_api_version( int modelIndex, string apiVersionValue )
        {
            // arrange
            var apiVersion = ApiVersion.Parse( apiVersionValue );
            var configuration = new HttpConfiguration();
            var models = CreateModels( configuration ).ToArray();

            configuration.MapVersionedODataRoutes( "odata", "api", models );

            // act
            var model = configuration.GetEdmModel( apiVersion );

            // assert
            model.Should().BeSameAs( models[modelIndex] );
        }

        const string RootContainerMappingsKey = "Microsoft.AspNet.OData.RootContainerMappingsKey";

        static IEnumerable<IEdmModel> CreateModels( HttpConfiguration configuration )
        {
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new List<Type>() { typeof( ControllerV1 ), typeof( ControllerV2 ) };

            controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.AddApiVersioning();

            var builder = new VersionedODataModelBuilder( configuration );

            return builder.GetEdmModels();
        }

        static IReadOnlyList<IODataRoutingConvention> GetRoutingConventions( HttpConfiguration configuration, ODataRoute route )
        {
            var routes = configuration.Routes;
            var pairs = new KeyValuePair<string, IHttpRoute>[routes.Count];

            routes.CopyTo( pairs, 0 );

            var key = pairs.Single( p => p.Value == route ).Key;
            var serviceProviders = (ConcurrentDictionary<string, IServiceProvider>) configuration.Properties[RootContainerMappingsKey];
            var routingConventions = (IEnumerable<IODataRoutingConvention>) serviceProviders[key].GetService( typeof( IEnumerable<IODataRoutingConvention> ) );

            return routingConventions.ToArray();
        }

        [ApiVersion( "1.0" )]
        sealed class ControllerV1 : ODataController
        {
            public IHttpActionResult Get() => Ok();
        }

        [ApiVersion( "2.0" )]
        sealed class ControllerV2 : ODataController
        {
            public IHttpActionResult Get() => Ok();
        }
    }
}