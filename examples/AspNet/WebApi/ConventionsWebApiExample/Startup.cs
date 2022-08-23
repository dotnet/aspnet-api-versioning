namespace ApiVersioning.Examples;

using ApiVersioning.Examples.Controllers;
using Asp.Versioning.Conventions;
using Asp.Versioning.Routing;
using Owin;
using System.Web.Http;
using System.Web.Http.Routing;

public partial class Startup
{
    public void Configuration( IAppBuilder builder )
    {
        // we only need to change the default constraint resolver for services that want
        // urls with versioning like: ~/v{version}/{controller}
        var constraintResolver = new DefaultInlineConstraintResolver()
        {
            ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) },
        };
        var configuration = new HttpConfiguration();
        var httpServer = new HttpServer( configuration );

        configuration.AddApiVersioning(
            options =>
            {
                // reporting api versions will return the headers
                // "api-supported-versions" and "api-deprecated-versions"
                options.ReportApiVersions = true;

                options.Conventions.Controller<ValuesController>().HasApiVersion( 1, 0 );

                options.Conventions.Controller<Values2Controller>()
                                   .HasApiVersion( 2, 0 )
                                   .HasApiVersion( 3, 0 )
                                   .Action( c => c.GetV3() ).MapToApiVersion( 3, 0 )
                                   .Action( c => c.GetV3( default ) ).MapToApiVersion( 3, 0 );

                options.Conventions.Controller<HelloWorldController>()
                                   .HasApiVersion( 1, 0 )
                                   .HasApiVersion( 2, 0 )
                                   .AdvertisesApiVersion( 3, 0 );
            } );

        configuration.MapHttpAttributeRoutes( constraintResolver );
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