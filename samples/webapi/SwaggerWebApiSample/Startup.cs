[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using global::Owin;
    using Microsoft.Web.Http.Routing;
    using Swashbuckle.Application;
    using System.IO;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Web.Http.Routing;

    /// <summary>
    /// Represents the startup process for the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configures the application using the provided builder.
        /// </summary>
        /// <param name="builder">The current application builder.</param>
        public void Configuration( IAppBuilder builder )
        {
            // we only need to change the default constraint resolver for services that want urls with versioning like: ~/v{version}/{controller}
            var constraintResolver = new DefaultInlineConstraintResolver() { ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) } };
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );

            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            configuration.AddApiVersioning( o => o.ReportApiVersions = true );
            configuration.MapHttpAttributeRoutes( constraintResolver );

            // add the versioned IApiExplorer and capture the strongly-typed implementation (e.g. VersionedApiExplorer vs IApiExplorer)
            // note: the specified format code will format the version as "'v'major[.minor][-status]"
            var apiExplorer = configuration.AddVersionedApiExplorer( o => o.GroupNameFormat = "'v'VVV" );

            configuration.EnableSwagger(
                            "{apiVersion}/swagger",
                            swagger =>
                            {
                                // build a swagger document and endpoint for each discovered API version
                                swagger.MultipleApiVersions(
                                    ( apiDescription, version ) => apiDescription.GetGroupName() == version,
                                    info =>
                                    {
                                        foreach ( var group in apiExplorer.ApiDescriptions )
                                        {
                                            var description = "A sample application with Swagger, Swashbuckle, and API versioning.";

                                            if ( group.IsDeprecated )
                                            {
                                                description += " This API version has been deprecated.";
                                            }

                                            info.Version( group.Name, $"Sample API {group.ApiVersion}" )
                                                .Contact( c => c.Name( "Bill Mei" ).Email( "bill.mei@somewhere.com" ) )
                                                .Description( description )
                                                .License( l => l.Name( "MIT" ).Url( "https://opensource.org/licenses/MIT" ) )
                                                .TermsOfService( "Shareware" );
                                        }
                                    } );
                                
                                // add a custom operation filter which sets default values
                                swagger.OperationFilter<SwaggerDefaultValues>();

                                // integrate xml comments
                                swagger.IncludeXmlComments( XmlCommentsFilePath );
                            } )
                         .EnableSwaggerUi( swagger => swagger.EnableDiscoveryUrlSelector() );

            builder.UseWebApi( httpServer );
        }

        static string XmlCommentsFilePath
        {
            get
            {
                var basePath = System.AppDomain.CurrentDomain.RelativeSearchPath;
                var fileName = typeof( Startup ).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine( basePath, fileName );
            }
        }
    }
}