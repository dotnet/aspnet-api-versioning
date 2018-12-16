namespace Microsoft.AspNetCore.OData.Conventions
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.AspNetCore.OData.Conventions.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.DependencyInjection;
    using System.Reflection;

    public class ConventionsFixture : ODataFixture
    {
        public ConventionsFixture()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( PeopleController ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( People2Controller ).GetTypeInfo() );
            FilteredControllerTypes.Add( typeof( CustomersController ).GetTypeInfo() );
        }

        protected override void OnAddApiVersioning( ApiVersioningOptions options )
        {
            options.ReportApiVersions = true;
            options.Conventions.Controller<OrdersController>()
                               .HasApiVersion( 1, 0 );
            options.Conventions.Controller<PeopleController>()
                               .HasApiVersion( 1, 0 )
                               .HasApiVersion( 2, 0 )
                               .Action( c => c.Patch( default, null, null ) ).MapToApiVersion( 2, 0 );
            options.Conventions.Controller<People2Controller>()
                               .HasApiVersion( 3, 0 );
            options.Conventions.Controller<CustomersController>()
                                       .Action( c => c.Get() ).HasApiVersion( 2, 0 ).HasApiVersion( 3, 0 )
                                       .Action( c => c.Get( default ) ).HasApiVersion( 1, 0 ).HasApiVersion( 2, 0 ).HasApiVersion( 3, 0 )
                                       .Action( c => c.Post( default ) ).HasApiVersion( 1, 0 ).HasApiVersion( 2, 0 ).HasApiVersion( 3, 0 )
                                       .Action( c => c.Put( default, default ) ).HasApiVersion( 3, 0 )
                                       .Action( c => c.Delete( default ) ).IsApiVersionNeutral();
        }

        protected override void OnConfigureRoutes( IRouteBuilder routeBuilder )
        {
            var modelBuilder = routeBuilder.ServiceProvider.GetRequiredService<VersionedODataModelBuilder>();
            var models = modelBuilder.GetEdmModels();

            routeBuilder.MapVersionedODataRoutes( "odata", "api", models );
            routeBuilder.MapVersionedODataRoutes( "odata-bypath", "v{version:apiVersion}", models );
        }
    }
}