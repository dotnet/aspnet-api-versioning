namespace Microsoft.AspNetCore.OData.Advanced
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.OData.Advanced.Controllers;
    using Microsoft.AspNetCore.OData.Configuration;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    public class AdvancedFixture : ODataFixture
    {
        public AdvancedFixture()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Orders2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( Orders3Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( PeopleController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( People2Controller ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options )
        {
            options.ReportApiVersions = true;
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader(),
                new HeaderApiVersionReader( "api-version", "x-ms-version" ) );
        }

        protected override void OnConfigureRoutes( IRouteBuilder routeBuilder )
        {
            var modelBuilder = routeBuilder.ServiceProvider.GetRequiredService<VersionedODataModelBuilder>();

            modelBuilder.ModelConfigurations.Clear();
            modelBuilder.ModelConfigurations.Add( new PersonModelConfiguration() );
            modelBuilder.ModelConfigurations.Add( new OrderModelConfiguration( supportedApiVersion: new ApiVersion( 2, 0 ) ) );

            var models = modelBuilder.GetEdmModels();

            routeBuilder.MapVersionedODataRoutes( "odata", "api", models );
        }
    }
}