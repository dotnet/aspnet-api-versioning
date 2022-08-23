namespace ApiVersioning.Examples;

using Asp.Versioning;
using Asp.Versioning.Routing;
using Owin;
using Swashbuckle.Application;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Routing;

/// <summary>
/// Represents the startup process for the application.
/// </summary>
public partial class Startup
{
    /// <summary>
    /// Configures the application using the provided builder.
    /// </summary>
    /// <param name="builder">The current application builder.</param>
    public void Configuration( IAppBuilder builder )
    {
        // we only need to change the default constraint resolver for services that want urls with versioning like: ~/v{version}/{controller}
        var constraintResolver = new DefaultInlineConstraintResolver()
        {
            ConstraintMap = { ["apiVersion"] = typeof( ApiVersionRouteConstraint ) },
        };
        var configuration = new HttpConfiguration();
        var httpServer = new HttpServer( configuration );

        configuration.AddApiVersioning( options =>
        {
            // reporting api versions will return the headers
            // "api-supported-versions" and "api-deprecated-versions"
            options.ReportApiVersions = true;

            options.Policies.Sunset( 0.9 )
                            .Effective( DateTimeOffset.Now.AddDays( 60 ) )
                            .Link( "policy.html" )
                                .Title( "Versioning Policy" )
                                .Type( "text/html" );
        } );
        configuration.MapHttpAttributeRoutes( constraintResolver );

        // add the versioned IApiExplorer and capture the strongly-typed
        // implementation (e.g. VersionedApiExplorer vs IApiExplorer)
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        var apiExplorer = configuration.AddVersionedApiExplorer(
            options =>
            {
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            } );

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
                        var description = new StringBuilder( "A sample application with OpenAPI, Swashbuckle, and API versioning." );

                        if ( group.IsDeprecated )
                        {
                            description.Append( " This API version has been deprecated." );
                        }

                        if ( group.SunsetPolicy is SunsetPolicy policy )
                        {
                            if ( policy.Date is DateTimeOffset when )
                            {
                                description.Append( " The API will be sunset on " )
                                           .Append( when.Date.ToShortDateString() )
                                           .Append( '.' );
                            }

                            if ( policy.HasLinks )
                            {
                                description.AppendLine();

                                for ( var i = 0; i < policy.Links.Count; i++ )
                                {
                                    var link = policy.Links[i];

                                    if ( link.Type == "text/html" )
                                    {
                                        description.AppendLine();

                                        if ( link.Title.HasValue )
                                        {
                                            description.Append( link.Title.Value ).Append( ": " );
                                        }

                                        description.Append( link.LinkTarget.OriginalString );
                                    }
                                }
                            }
                        }

                        info.Version( group.Name, $"Example API {group.ApiVersion}" )
                            .Contact( c => c.Name( "Bill Mei" ).Email( "bill.mei@somewhere.com" ) )
                            .Description( description.ToString() )
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

    /// <summary>
    /// Get the root content path.
    /// </summary>
    /// <value>The root content path of the application.</value>
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

    private static string XmlCommentsFilePath
    {
        get
        {
            var fileName = typeof( Startup ).GetTypeInfo().Assembly.GetName().Name + ".xml";
            return Path.Combine( ContentRootPath, fileName );
        }
    }
}