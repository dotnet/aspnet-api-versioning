namespace ApiVersioning.Examples;

using Asp.Versioning;
using Asp.Versioning.Conventions;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;
using Newtonsoft.Json.Serialization;
using Owin;
using Swashbuckle.Application;
using System.IO;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using static Microsoft.AspNet.OData.Query.AllowedQueryOptions;

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
        var configuration = new HttpConfiguration();
        var httpServer = new HttpServer( configuration );

        // note: this example application intentionally only illustrates the
        // bare minimum configuration for OpenAPI with partial OData support.
        // see the OpenAPI or OData OpenAPI examples for additional options.

        configuration.EnableDependencyInjection();
        configuration.Select();
        configuration.AddApiVersioning();

        // note: this is required to make the default swagger json
        // settings match the odata conventions applied by EnableLowerCamelCase()
        configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
            new CamelCasePropertyNamesContractResolver();

        // NOTE: when you mix OData and non-Data controllers in Web API, it's RECOMMENDED to only use
        // convention-based routing. using attribute routing may not work as expected due to limitations
        // in the underlying routing system. the order of route registration is important as well.
        //
        // for example:
        //
        // configuration.MapVersionedODataRoute( "odata", "api", modelBuilder );
        // configuration.Routes.MapHttpRoute( "Default", "api/{controller}/{id}", new { id = RouteParameter.Optional } );
        //
        // for more information see the advanced OData example
        configuration.MapHttpAttributeRoutes();

        // add the versioned IApiExplorer and capture the strongly-typed implementation (e.g. ODataApiExplorer vs IApiExplorer)
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        var apiExplorer = configuration.AddODataApiExplorer(
            options =>
            {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // configure query options (which cannot otherwise be configured by OData conventions)
                options.QueryOptions.Controller<BooksController>()
                                    .Action( c => c.Get( default ) )
                                        .Allow( Skip | Count )
                                        .AllowTop( 100 )
                                        .AllowOrderBy( "title", "published" );

                // applies model bound settings implicitly using an ad hoc EDM. alternatively, you can create your own
                // IModelConfiguration + IODataQueryOptionsConvention for full control over what goes in the ad hoc EDM.
                options.AdHocModelBuilder.ModelConfigurations.Add( new ImplicitModelBoundSettingsConvention() );
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
                        var description = new StringBuilder( "A sample application with some OData, OpenAPI, Swashbuckle, and API versioning." );

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

                        info.Version( group.Name, $"Sample API {group.ApiVersion}" )
                            .Contact( c => c.Name( "Bill Mei" ).Email( "bill.mei@somewhere.com" ) )
                            .Description( description.ToString() )
                            .License( l => l.Name( "MIT" ).Url( "https://opensource.org/licenses/MIT" ) )
                            .TermsOfService( "Shareware" );
                    }
                } );

                // add a custom operation filter which documents the implicit API version parameter
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