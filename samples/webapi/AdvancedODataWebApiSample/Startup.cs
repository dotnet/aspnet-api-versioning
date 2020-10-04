[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using global::Owin;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.Examples.Configuration;
    using Microsoft.OData;
    using Microsoft.Web.Http.Versioning;
    using System;
    using System.Web.Http;
    using static System.Web.Http.RouteParameter;

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

                    // allows a client to make a request without specifying an api version. the value of
                    // options.DefaultApiVersion will be 'assumed'; this is meant to grandfather in legacy apis
                    options.AssumeDefaultVersionWhenUnspecified = true;

                    // allow multiple locations to request an api version
                    options.ApiVersionReader = ApiVersionReader.Combine(
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

            // NOTE: when you mix OData and non-Data controllers in Web API, it's RECOMMENDED to only use
            // convention-based routing. using attribute routing may not work as expected due to limitations
            // in the underlying routing system. the order of route registration is important as well.
            //
            // DO NOT use configuration.MapHttpAttributeRoutes();
            configuration.MapVersionedODataRoute( "odata", "api", modelBuilder.GetEdmModels() );
            configuration.Routes.MapHttpRoute( "orders", "api/{controller}/{id}", new { id = Optional } );

            configuration.Formatters.Remove( configuration.Formatters.XmlFormatter );
            appBuilder.UseWebApi( httpServer );
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