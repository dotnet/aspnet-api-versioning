[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using Configuration;
    using Controllers;
    using global::Owin;
    using Microsoft.OData.Edm;
    using Microsoft.Web.Http;
    using Microsoft.Web.Http.Versioning.Conventions;
    using Microsoft.Web.OData.Builder;
    using System.Linq;
    using System.Web.Http;
    using System.Web.OData.Batch;
    using System.Web.OData.Builder;
    using System.Web.OData.Extensions;

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
            configuration.EnableCaseInsensitive( true );
            configuration.EnableUnqualifiedNameCall( true );

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
            var v1 = new ApiVersion( 1, 0 );
            var modelV1 = models.Single( m => m.GetAnnotationValue<ApiVersionAnnotation>( m ).ApiVersion == v1 );

            configuration.MapVersionedODataRoutes( "odata", "api", models, batchHandler );
            configuration.MapVersionedODataRoute( "odata-v1", "v1", modelV1, v1, batchHandler );
            appBuilder.UseWebApi( httpServer );
        }
    }
}