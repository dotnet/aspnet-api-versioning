﻿namespace Microsoft.AspNetCore.OData.Basic
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.OData.Basic.Controllers.Classic;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    public class BasicFixture : ODataFixture
    {
        public BasicFixture()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( PeopleController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( People2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( CustomersController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( WeatherForecastsController ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options ) => options.ReportApiVersions = true;

        protected override void OnConfigureRoutes( IRouteBuilder routeBuilder )
        {
            var modelBuilder = routeBuilder.ServiceProvider.GetRequiredService<VersionedODataModelBuilder>();
            var models = modelBuilder.GetEdmModels();

            routeBuilder.MapVersionedODataRoute( "odata", "api", models );
            routeBuilder.MapVersionedODataRoute( "odata-bypath", "v{version:apiVersion}", models );
        }
    }
}