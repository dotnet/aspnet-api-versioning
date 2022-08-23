namespace ApiVersioning.Examples;

using ApiVersioning.Examples.Configuration;
using ApiVersioning.Examples.Controllers;
using Asp.Versioning.Conventions;
using Asp.Versioning.OData;
using Microsoft.OData;
using Owin;
using System.Web.Http;

public partial class Startup
{
    public void Configuration( IAppBuilder appBuilder )
    {
        var configuration = new HttpConfiguration();
        var httpServer = new HttpServer( configuration );

        configuration.AddApiVersioning(
            options =>
            {
                    // reporting api versions will return the headers
                    // "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;

                    // apply api versions using conventions rather than attributes
                    options.Conventions.Controller<OrdersController>()
                                   .HasApiVersion( 1, 0 );

                options.Conventions.Controller<PeopleController>()
                                   .HasApiVersion( 1, 0 )
                                   .HasApiVersion( 2, 0 )
                                   .Action( c => c.Patch( default, default, default ) ).MapToApiVersion( 2, 0 );

                options.Conventions.Controller<People2Controller>()
                                   .HasApiVersion( 3, 0 );
            } );

        var modelBuilder = new VersionedODataModelBuilder( configuration )
        {
            ModelConfigurations =
            {
                new PersonModelConfiguration(),
                new OrderModelConfiguration(),
            },
        };

        // INFO: you do NOT and should NOT use both the query string and url segment methods together.
        // this configuration is merely illustrating that they can coexist and allows you to easily
        // experiment with either configuration. one of these would be removed in a real application.

        // WHEN VERSIONING BY: query string, header, or media type
        configuration.MapVersionedODataRoute( "odata", "api", modelBuilder );

        // WHEN VERSIONING BY: url segment
        configuration.MapVersionedODataRoute( "odata-bypath", "api/v{apiVersion}", modelBuilder );

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