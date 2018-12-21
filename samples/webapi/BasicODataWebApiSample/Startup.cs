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
    using System;
    using System.Web.Http;
    using static Microsoft.OData.ODataUrlKeyDelimiter;
    using static Microsoft.OData.ServiceLifetime;

    public class Startup
    {
        public void Configuration( IAppBuilder appBuilder )
        {
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );

            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            configuration.AddApiVersioning( options => options.ReportApiVersions = true );

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

            // NOTE: you do NOT and should NOT use both the query string and url segment methods together.
            // this configuration is merely illustrating that they can coexist and allows you to easily
            // experiment with either configuration. one of these would be removed in a real application.
            configuration.MapVersionedODataRoutes( "odata", "api", models, ConfigureContainer, batchHandler );
            configuration.MapVersionedODataRoutes( "odata-bypath", "api/v{apiVersion}", models, ConfigureContainer );

            appBuilder.UseWebApi( httpServer );
        }

        static void ConfigureContainer( IContainerBuilder builder )
        {
            builder.AddService<IODataPathHandler>( Singleton, sp => new DefaultODataPathHandler() { UrlKeyDelimiter = Parentheses } );
            builder.AddService<ODataUriResolver>( Singleton, sp => new UnqualifiedCallAndEnumPrefixFreeResolver() { EnableCaseInsensitive = true } );
        }

        public static string ContentRootPath
        {
            get
            {
                var app = AppDomain.CurrentDomain;

                if ( string.IsNullOrEmpty( app.RelativeSearchPath ) )
                {
                    return app.BaseDirectory;
                }

                return app.RelativeSearchPath;
            }
        }
    }
}