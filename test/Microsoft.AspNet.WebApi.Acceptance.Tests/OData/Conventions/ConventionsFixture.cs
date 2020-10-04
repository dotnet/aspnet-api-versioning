namespace Microsoft.AspNet.OData.Conventions
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Configuration;
    using Microsoft.AspNet.OData.Conventions.Controllers;
    using Microsoft.Web.Http.Versioning.Conventions;
    using System.Web.Http;

    public class ConventionsFixture : ODataFixture
    {
        public ConventionsFixture()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ) );
            FilteredControllerTypes.Add( typeof( PeopleController ) );
            FilteredControllerTypes.Add( typeof( People2Controller ) );
            FilteredControllerTypes.Add( typeof( CustomersController ) );

            Configuration.AddApiVersioning(
                options =>
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

                } );

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

            Configuration.MapVersionedODataRoute( "odata", "api", models );
            Configuration.MapVersionedODataRoute( "odata-bypath", "v{apiVersion}", models );
            Configuration.EnsureInitialized();
        }
    }
}