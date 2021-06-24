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
    using Microsoft.OData.Edm;
    using Microsoft.Simulators;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Xunit;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

    public class IRouteBuilderExtensionsTest
    {
        [Theory]
        [InlineData( null )]
        [InlineData( "api" )]
        [InlineData( "v{version:apiVersion}" )]
        public void map_versioned_odata_route_should_return_expected_result( string routePrefix )
        {
            // arrange
            var routeName = "odata";
            var app = NewApplicationBuilder();
            var route = default( ODataRoute );
            var perRequestContainer = app.ApplicationServices.GetRequiredService<IPerRouteContainer>();
            var modelBuilder = app.ApplicationServices.GetRequiredService<VersionedODataModelBuilder>();

            modelBuilder.ModelConfigurations.Add( new TestModelConfiguration() );

            var models = modelBuilder.GetEdmModels();

            // act
            app.UseMvc( rb => route = rb.MapVersionedODataRoute( routeName, routePrefix, modelBuilder.GetEdmModels(), new DefaultODataBatchHandler() ) );

            // assert
            var rootContainer = perRequestContainer.GetODataRootContainer( routeName );
            var selector = rootContainer.GetRequiredService<IEdmModelSelector>();
            var routingConventions = rootContainer.GetRequiredService<IEnumerable<IODataRoutingConvention>>().ToArray();
            var batchHandler = perRequestContainer.GetODataRootContainer( routeName ).GetRequiredService<ODataBatchHandler>();

            selector.ApiVersions.Should().Equal(
                new[]
                {
                    new ApiVersion( 1, 0 ),
                    new ApiVersion( 2, 0 ),
                    new ApiVersion( 3, 0, "Beta" ),
                    new ApiVersion( 3, 0 )
                } );
            routingConventions[0].Should().BeOfType<VersionedAttributeRoutingConvention>();
            routingConventions[1].Should().BeOfType<VersionedMetadataRoutingConvention>();
            routingConventions.OfType<MetadataRoutingConvention>().Should().BeEmpty();
            route.PathRouteConstraint.RouteName.Should().Be( routeName );
            route.RoutePrefix.Should().Be( routePrefix );
            batchHandler.ODataRoute.Should().NotBeNull();
            batchHandler.ODataRouteName.Should().Be( routeName );
        }

        static ApplicationBuilder NewApplicationBuilder()
        {
            var services = new ServiceCollection();
            var testControllers = new TestApplicationPart(
                    typeof( TestsController ),
                    typeof( Tests2Controller ),
                    typeof( Tests3Controller ) );

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