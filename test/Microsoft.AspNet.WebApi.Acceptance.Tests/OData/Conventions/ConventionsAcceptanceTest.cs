namespace Microsoft.Web.OData.Conventions
{
    using Builder;
    using Configuration;
    using Controllers;
    using Http.Versioning.Conventions;
    using System.Web.Http;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;

    public abstract class ConventionsAcceptanceTest : ODataAcceptanceTest
    {
        protected ConventionsAcceptanceTest()
        {
            FilteredControllerTypes.Add( typeof( OrdersController ) );
            FilteredControllerTypes.Add( typeof( PeopleController ) );
            FilteredControllerTypes.Add( typeof( People2Controller ) );

            Configuration.AddApiVersioning(
                options =>
                {
                    options.ReportApiVersions = true;
                    options.Conventions.Controller<OrdersController>()
                                       .HasApiVersion( 1, 0 );
                    options.Conventions.Controller<PeopleController>()
                                       .HasApiVersion( 1, 0 )
                                       .HasApiVersion( 2, 0 )
                                       .Action( c => c.Patch( default( int ), null, null ) ).MapToApiVersion( 2, 0 );
                    options.Conventions.Controller<People2Controller>()
                                       .HasApiVersion( 3, 0 );
                } );

            var modelBuilder = new VersionedODataModelBuilder( Configuration )
            {
                ModelBuilderFactory = () => new ODataConventionModelBuilder().EnableLowerCamelCase(),
                ModelConfigurations =
                {
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration()
                }
            };
            var models = modelBuilder.GetEdmModels();

            Configuration.MapVersionedODataRoutes( "odata", "api", models );
            Configuration.MapVersionedODataRoutes( "odata-bypath", "v{apiVersion}", models );
            Configuration.EnsureInitialized();
        }
    }
}