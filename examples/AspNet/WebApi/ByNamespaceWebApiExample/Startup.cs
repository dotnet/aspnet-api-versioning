namespace ApiVersioning.Examples;

using Asp.Versioning.Conventions;
using Asp.Versioning.Routing;
using Owin;
using System.Web.Http;

public partial class Startup
{
    public void Configuration( IAppBuilder builder )
    {
        var configuration = new HttpConfiguration();
        var httpServer = new HttpServer( configuration );

        configuration.AddApiVersioning(
            options =>
            {
                    // reporting api versions will return the headers
                    // "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;

                    // automatically applies an api version based on the name of
                    // the defining controller's namespace
                    options.Conventions.Add( new VersionByNamespaceConvention() );
            } );

        // NOTE: you do NOT and should NOT use both the query string and url segment methods together.
        // this configuration is merely illustrating that they can coexist and allows you to easily
        // experiment with either configuration. one of these would be removed in a real application.
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