[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using global::Owin;
    using Microsoft.Web.Http.Routing;
    using Microsoft.Web.Http.Versioning.Conventions;
    using System.Web.Http;
    using System.Web.Http.Routing;

    public class Startup
    {
        public void Configuration( IAppBuilder builder )
        {
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );

            configuration.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;

                    // automatically applies an api version based on the name of the defining controller's namespace
                    options.Conventions.Add( new VersionByNamespaceConvention() );
                } );

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