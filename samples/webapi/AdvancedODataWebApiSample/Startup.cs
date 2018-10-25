[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using global::Owin;
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Batch;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Routing;
    using Microsoft.Examples.Configuration;
    using Microsoft.OData;
    using Microsoft.OData.UriParser;
    using Microsoft.Web.Http.Versioning;
    using System.Web.Http;
    using static Microsoft.OData.ODataUrlKeyDelimiter;
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
                ModelConfigurations =
                {
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration()
                }
            };
            var models = modelBuilder.GetEdmModels();
            var batchHandler = new DefaultODataBatchHandler( httpServer );

            configuration.MapVersionedODataRoutes( "odata", "api", models, ConfigureContainer, batchHandler );
            configuration.Routes.MapHttpRoute( "orders", "api/{controller}/{id}", new { id = Optional } );
            appBuilder.UseWebApi( httpServer );
        }

        static void ConfigureContainer( IContainerBuilder builder )
        {
            builder.AddService<IODataPathHandler>( Singleton, sp => new DefaultODataPathHandler() { UrlKeyDelimiter = Parentheses } );
            builder.AddService<ODataUriResolver>( Singleton, sp => new UnqualifiedCallAndEnumPrefixFreeResolver() { EnableCaseInsensitive = true } );
        }
    }
}