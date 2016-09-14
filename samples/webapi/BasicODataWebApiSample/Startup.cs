[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using Configuration;
    using global::Owin;
    using Microsoft.Web.OData.Builder;
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

            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            configuration.AddApiVersioning( o => o.ReportApiVersions = true );
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

            configuration.MapVersionedODataRoutes( "odata", "api", models, batchHandler );
            configuration.MapVersionedODataRoutes( "odata-bypath", "v{apiVersion}", models, batchHandler );
            appBuilder.UseWebApi( httpServer );
        }
    }
}