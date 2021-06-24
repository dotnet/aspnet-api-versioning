namespace Microsoft.AspNet.OData.Basic
{
    using Microsoft.AspNet.OData.Basic.Controllers;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Configuration;
    using System.Web.Http;

    public class BasicFixture : ODataFixture
    {
        public BasicFixture()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ) );
            FilteredControllerTypes.Add( typeof( PeopleController ) );
            FilteredControllerTypes.Add( typeof( People2Controller ) );
            FilteredControllerTypes.Add( typeof( CustomersController ) );
            FilteredControllerTypes.Add( typeof( WeatherForecastsController ) );

            Configuration.AddApiVersioning( options => options.ReportApiVersions = true );

            var modelBuilder = new VersionedODataModelBuilder( Configuration )
            {
                ModelConfigurations =
                {
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration(),
                    new CustomerModelConfiguration(),
                    new WeatherForecastModelConfiguration(),
                }
            };
            var models = modelBuilder.GetEdmModels();

            Configuration.MapVersionedODataRoute( "odata", "api", models );
            Configuration.MapVersionedODataRoute( "odata-bypath", "v{apiVersion}", models );
            Configuration.EnsureInitialized();
        }
    }
}