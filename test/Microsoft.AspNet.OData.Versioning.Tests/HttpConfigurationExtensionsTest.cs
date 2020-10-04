﻿namespace Microsoft.AspNet.OData
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.Extensions.DependencyInjection;
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
        [Theory]
        [InlineData( null )]
        [InlineData( "api" )]
        [InlineData( "v{apiVersion}" )]
        public void map_versioned_odata_route_should_return_expected_result( string routePrefix )
        {
            // arrange
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );
            var routeName = "odata";
            var batchTemplate = "$batch";
            var batchHandler = new DefaultODataBatchHandler( httpServer );
            var models = CreateModels( configuration );

            if ( !string.IsNullOrEmpty( routePrefix ) )
            {
                batchTemplate = routePrefix + "/" + batchTemplate;
            }

            // act
            var route = configuration.MapVersionedODataRoute( routeName, routePrefix, models, batchHandler );
            var batchRoute = configuration.Routes["odataBatch"];

            // assert
            var selector = GetODataRootContainer( configuration, routeName ).GetRequiredService<IEdmModelSelector>();
            var routingConventions = GetRoutingConventions( configuration, route );

            selector.ApiVersions.Should().Equal( new[] { new ApiVersion( 1, 0 ), new ApiVersion( 2, 0 ) } );
            routingConventions[0].Should().BeOfType<VersionedAttributeRoutingConvention>();
            routingConventions[1].Should().BeOfType<VersionedMetadataRoutingConvention>();
            routingConventions.OfType<MetadataRoutingConvention>().Should().BeEmpty();
            route.PathRouteConstraint.RouteName.Should().Be( routeName );
            route.RoutePrefix.Should().Be( routePrefix );
            batchRoute.Handler.Should().Be( batchHandler );
            batchRoute.RouteTemplate.Should().Be( batchTemplate );
        }

        static IEnumerable<IEdmModel> CreateModels( HttpConfiguration configuration )
        {
            var controllerTypeResolver = new Mock<IHttpControllerTypeResolver>();
            var controllerTypes = new List<Type>() { typeof( ControllerV1 ), typeof( ControllerV2 ) };

            controllerTypeResolver.Setup( ctr => ctr.GetControllerTypes( It.IsAny<IAssembliesResolver>() ) ).Returns( controllerTypes );
            configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), controllerTypeResolver.Object );
            configuration.AddApiVersioning();

            var builder = new VersionedODataModelBuilder( configuration )
            {
                DefaultModelConfiguration = ( b, v, r ) => b.EntitySet<TestEntity>( "Tests" ),
            };

            return builder.GetEdmModels();
        }

        static IServiceProvider GetODataRootContainer( HttpConfiguration configuration, string routeName )
        {
            const string RootContainerMappingsKey = "Microsoft.AspNet.OData.RootContainerMappingsKey";
            var serviceProviders = (ConcurrentDictionary<string, IServiceProvider>) configuration.Properties[RootContainerMappingsKey];
            return serviceProviders[routeName];
        }

        static IReadOnlyList<IODataRoutingConvention> GetRoutingConventions( HttpConfiguration configuration, ODataRoute route )
        {
            var routes = configuration.Routes;
            var pairs = new KeyValuePair<string, IHttpRoute>[routes.Count];

            routes.CopyTo( pairs, 0 );

            var key = pairs.Single( p => p.Value == route ).Key;
            var serviceProvider = GetODataRootContainer( configuration, key );
            var routingConventions = (IEnumerable<IODataRoutingConvention>) serviceProvider.GetService( typeof( IEnumerable<IODataRoutingConvention> ) );

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