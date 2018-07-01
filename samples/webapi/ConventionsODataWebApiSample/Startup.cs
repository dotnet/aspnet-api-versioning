[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using Configuration;
    using Controllers;
    using global::Owin;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.Web.Http.Versioning.Conventions;
    using System.Web.Http;

    public class Startup
    {
        public void Configuration( IAppBuilder appBuilder )
        {
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );

            configuration.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;

                    // apply api versions using conventions rather than attributes
                    options.Conventions.Controller<OrdersController>()
                                       .HasApiVersion( 1, 0 );

                    options.Conventions.Controller<PeopleController>()
                                       .HasApiVersion( 1, 0 )
                                       .HasApiVersion( 2, 0 )
                                       .Action( c => c.Patch( default( int ), null, null ) ).MapToApiVersion( 2, 0 );

                    options.Conventions.Controller<People2Controller>()
                                       .HasApiVersion( 3, 0 );
                } );

            var modelBuilder = new VersionedODataModelBuilder( configuration )
            {
                ModelBuilderFactory = () => new ODataConventionModelBuilder().EnableLowerCamelCase(),
                ModelConfigurations =
                {
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration()
                }
            };
            var models = modelBuilder.GetEdmModels();
            var batchHandler = new DefaultODataBatchHandler( httpServer );

            configuration.MapVersionedODataRoutes( "odata", "api", models, batchHandler );
            configuration.MapVersionedODataRoutes( "odata-bypath", "v{apiVersion}", models );
            appBuilder.UseWebApi( httpServer );
        }
    }
}