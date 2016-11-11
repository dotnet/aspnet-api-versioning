[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using global::Owin;
    using Microsoft.Web.Http.Routing;
    using System.Web.Http;
    using System.Web.Http.Routing;
    using static System.Web.Http.RouteParameter;

    public class Startup
    {
        public void Configuration( IAppBuilder builder )
        {
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );

            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            configuration.AddApiVersioning( o => o.ReportApiVersions = true );

            configuration.Routes.MapHttpRoute(
                "VersionedQueryString",
                "api/{controller}/{accountId}",
                defaults: null );

            configuration.Routes.MapHttpRoute(
                "VersionedUrl",
                "v{apiVersion}/{controller}/{accountId}",
                defaults: null,
                constraints: new { apiVersion = new ApiVersionRouteConstraint() } );

            builder.UseWebApi( httpServer );
        }
    }
}