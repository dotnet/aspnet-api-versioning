namespace Microsoft.AspNet.OData.Extensions
{
    using FluentAssertions;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.AspNet.OData.Routing.Conventions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;
    using Microsoft.Simulators;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Xunit;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

    public class IRouteBuilderExtensionsTest
    {
        [Fact]
        public void map_versioned_odata_route_should_return_expected_result()
        {
            // arrange
            var routeName = "odata";
            var routePrefix = "api/v3";
            var modelBuilder = new ODataModelBuilder();
            var modelConfiguration = new TestModelConfiguration();
            var apiVersion = new ApiVersion( 3, 0 );
            var batchHandler = new DefaultODataBatchHandler();
            var app = NewApplicationBuilder();
            var route = default( ODataRoute );

            modelConfiguration.Apply( modelBuilder, apiVersion );

            var model = modelBuilder.GetEdmModel();

            // act
            app.UseMvc( r => route = r.MapVersionedODataRoute( routeName, routePrefix, model, apiVersion, batchHandler ) );

            var perRequestContainer = app.ApplicationServices.GetRequiredService<IPerRouteContainer>();
            var serviceProvider = perRequestContainer.GetODataRootContainer( route.Name );
            var routingConventions = serviceProvider.GetRequiredService<IEnumerable<IODataRoutingConvention>>().ToArray();
            var constraint = (VersionedODataPathRouteConstraint) route.PathRouteConstraint;

            // assert
            routingConventions[0].Should().BeOfType<VersionedAttributeRoutingConvention>();
            routingConventions[1].Should().BeOfType<VersionedMetadataRoutingConvention>();
            routingConventions.OfType<MetadataRoutingConvention>().Should().BeEmpty();
            constraint.RouteName.Should().Be( routeName );
            route.RoutePrefix.Should().Be( routePrefix );
            batchHandler.ODataRoute.Should().NotBeNull();
            batchHandler.ODataRouteName.Should().Be( routeName );
        }

        [Fact]
        public void map_versioned_odata_routes_should_return_expected_results()
        {
            // arrange
            var routeName = "odata";
            var routePrefix = "api";
            var app = NewApplicationBuilder();
            var routes = default( IReadOnlyList<ODataRoute> );
            var perRequestContainer = app.ApplicationServices.GetRequiredService<IPerRouteContainer>();
            var modelBuilder = app.ApplicationServices.GetRequiredService<VersionedODataModelBuilder>();

            modelBuilder.ModelConfigurations.Add( new TestModelConfiguration() );

            var models = modelBuilder.GetEdmModels();

            // act
            app.UseMvc( r => routes = r.MapVersionedODataRoutes( routeName, routePrefix, models, () => new DefaultODataBatchHandler() ) );

            // assert
            foreach ( var route in routes )
            {
                if ( !( route.PathRouteConstraint is VersionedODataPathRouteConstraint constraint ) )
                {
                    continue;
                }

                var apiVersion = constraint.ApiVersion;
                var versionedRouteName = routeName + "-" + apiVersion.ToString();
                var rootContainer = perRequestContainer.GetODataRootContainer( versionedRouteName );
                var routingConventions = rootContainer.GetRequiredService<IEnumerable<IODataRoutingConvention>>().ToArray();
                var batchHandler = perRequestContainer.GetODataRootContainer( versionedRouteName ).GetRequiredService<ODataBatchHandler>();

                routingConventions[0].Should().BeOfType<VersionedAttributeRoutingConvention>();
                routingConventions[1].Should().BeOfType<VersionedMetadataRoutingConvention>();
                routingConventions.OfType<MetadataRoutingConvention>().Should().BeEmpty();
                constraint.RouteName.Should().Be( versionedRouteName );
                route.RoutePrefix.Should().Be( routePrefix );
                batchHandler.ODataRoute.Should().NotBeNull();
                batchHandler.ODataRouteName.Should().Be( versionedRouteName );
            }
        }

        static ApplicationBuilder NewApplicationBuilder()
        {
            var services = new ServiceCollection();
            var testControllers = new TestApplicationPart(
                    typeof( TestsController ),
                    typeof( TestsController2 ),
                    typeof( TestsController3 ) );

            services.AddLogging();
            services.Add( Singleton( new DiagnosticListener( "test" ) ) );
            services.AddMvcCore( options => options.EnableEndpointRouting = false )
                    .ConfigureApplicationPartManager( apm => apm.ApplicationParts.Add( testControllers ) );
            services.AddApiVersioning();
            services.AddOData().EnableApiVersioning();

            return new ApplicationBuilder( services.BuildServiceProvider() );
        }
    }
}