namespace Microsoft.AspNet.OData.Basic
{
    using Microsoft.AspNet.OData.Basic.Controllers;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Configuration;
    using Microsoft.OData.UriParser;
    using System.Web.Http;
    using static Microsoft.OData.ServiceLifetime;

    public class BasicFixture : ODataFixture
    {
        public BasicFixture()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ) );
            FilteredControllerTypes.Add( typeof( PeopleController ) );
            FilteredControllerTypes.Add( typeof( People2Controller ) );
            FilteredControllerTypes.Add( typeof( CustomersController ) );

            Configuration.AddApiVersioning( options => options.ReportApiVersions = true );

            var modelBuilder = new VersionedODataModelBuilder( Configuration )
            {
                ModelConfigurations =
                {
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration(),
                    new CustomerModelConfiguration(),
                }
            };
            var models = modelBuilder.GetEdmModels();

            Configuration.MapVersionedODataRoutes( "odata", "api", models, builder => builder.AddService( Singleton, typeof( ODataUriResolver ), sp => UriResolver ) );
            Configuration.MapVersionedODataRoutes( "odata-bypath", "v{apiVersion}", models, builder => builder.AddService( Singleton, typeof( ODataUriResolver ), sp => UriResolver ) );
            Configuration.EnsureInitialized();
        }
    }
}