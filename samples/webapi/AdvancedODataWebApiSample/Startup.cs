[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using Configuration;
    using global::Owin;
    using Microsoft.OData;
    using Microsoft.OData.UriParser;
    using Microsoft.Web.Http.Versioning;
    using Microsoft.Web.OData.Builder;
    using System.Web.Http;
    using System.Web.OData.Batch;
    using System.Web.OData.Builder;
    using static Microsoft.OData.ServiceLifetime;
    using static System.Web.Http.RouteParameter;

    public class Startup
    {
        public void Configuration( IAppBuilder appBuilder )
        {
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );

            configuration.AddApiVersioning(
                o =>
                {
                    o.ReportApiVersions = true;
                    o.AssumeDefaultVersionWhenUnspecified = true;
                    o.ApiVersionReader = ApiVersionReader.Combine(
                        new QueryStringApiVersionReader(),
                        new HeaderApiVersionReader( "api-version", "x-ms-version" ) );
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

            configuration.MapVersionedODataRoutes( "odata", "api", models, ConfigureODataServices, batchHandler );
            configuration.Routes.MapHttpRoute( "orders", "api/{controller}/{id}", new { id = Optional } );
            appBuilder.UseWebApi( httpServer );
        }

        static void ConfigureODataServices( IContainerBuilder builder )
        {
            builder.AddService( Singleton, typeof( ODataUriResolver ), sp => new CaseInsensitiveODataUriResolver() );
        }
    }
}