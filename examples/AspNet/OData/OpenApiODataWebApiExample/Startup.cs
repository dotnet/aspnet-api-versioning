namespace ApiVersioning.Examples;

using ApiVersioning.Examples.Configuration;
using Asp.Versioning;
using Asp.Versioning.Conventions;
using Asp.Versioning.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.Primitives;
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

        configuration.AddApiVersioning( options =>
        {
            // reporting api versions will return the headers
            // "api-supported-versions" and "api-deprecated-versions"
            options.ReportApiVersions = true;

            options.Policies.Deprecate( 0.9 )
                            .Effective( DateTimeOffset.Now )
                            .Link( "policy.html" )
                                .Title( "Version Deprecation Policy" )
                                .Type( "text/html" );

            options.Policies.Sunset( 0.9 )
                            .Effective( DateTimeOffset.Now.AddDays( 60 ) )
                            .Link( "policy.html" )
                                .Title( "Version Sunset Policy" )
                                .Type( "text/html" );
        } );

        // note: this is required to make the default swagger json
        // settings match the odata conventions applied by EnableLowerCamelCase()
        configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver =
            new CamelCasePropertyNamesContractResolver();

        var modelBuilder = new VersionedODataModelBuilder( configuration )
        {
            ModelConfigurations =
            {
                new AllConfigurations(),
                new PersonModelConfiguration(),
                new OrderModelConfiguration(),
                new ProductConfiguration(),
                new SupplierConfiguration(),
            },
        };

        // global odata query options
        configuration.Count();

        // INFO: you do NOT and should NOT use both the query string and url segment methods together.
        // this configuration is merely illustrating that they can coexist and allows you to easily
        // experiment with either configuration. one of these would be removed in a real application.
        //
        // INFO: only pass the route prefix to GetEdmModels if you want to split the models; otherwise, both routes contain all models

        // WHEN VERSIONING BY: query string, header, or media type
        configuration.MapVersionedODataRoute( "odata", "api", modelBuilder );

        // WHEN VERSIONING BY: url segment
        // configuration.MapVersionedODataRoute( "odata-bypath", "api/v{apiVersion}", models );

        // add the versioned IApiExplorer and capture the strongly-typed implementation (e.g. ODataApiExplorer vs IApiExplorer)
        // note: the specified format code will format the version as "'v'major[.minor][-status]"
        var apiExplorer = configuration.AddODataApiExplorer(
            options =>
            {
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;

                // configure query options (which cannot otherwise be configured by OData conventions)
                options.QueryOptions.Controller<V2.PeopleController>()
                                    .Action( c => c.Get( default ) )
                                        .Allow( Skip | Count )
                                        .AllowTop( 100 )
                                        .AllowOrderBy( "firstName", "lastName" );

                options.QueryOptions.Controller<V3.PeopleController>()
                                    .Action( c => c.Get( default ) )
                                        .Allow( Skip | Count )
                                        .AllowTop( 100 )
                                        .AllowOrderBy( "firstName", "lastName" );
            } );

        configuration.EnableSwagger(
            "{apiVersion}/swagger",
            swagger =>
            {
                // build a swagger document and endpoint for each discovered API version
                swagger.MultipleApiVersions(
                ( apiDescription, version ) => apiDescription.GroupName == version,
                info =>
                {
                    foreach ( var group in apiExplorer.ApiDescriptions )
                    {
                        var description = new StringBuilder( "A sample application with OData, OpenAPI, Swashbuckle, and API versioning." );
                        var links = new List<LinkHeaderValue>();

                        if ( group.IsDeprecated )
                        {
                            description.Append( " The API " );

                            if ( group.DeprecationPolicy?.Date is { } when )
                            {
                                description.Append( when < DateTimeOffset.Now ? "will be" : "was" )
                                           .Append( " deprecated on " )
                                           .Append( when.Date.ToShortDateString() );
                            }
                            else
                            {
                                description.Append( "has been deprecated" );
                            }

                            description.Append( '.' );

                            if ( group.DeprecationPolicy is { } deprecation && deprecation.HasLinks )
                            {
                                links.AddRange( deprecation.Links );
                            }
                        }

                        if ( group.SunsetPolicy is { } sunset )
                        {
                            if ( sunset.Date is { } when )
                            {
                                description.Append( " The API " )
                                           .Append( when < DateTimeOffset.Now ? "will be" : "was" )
                                           .Append( " sunset on " )
                                           .Append( when.Date.ToShortDateString() )
                                           .Append( '.' );
                            }

                            if ( sunset.HasLinks )
                            {
                                links.AddRange( sunset.Links );
                            }
                        }

                        description.AppendLine();

                        if ( links.Count > 0 )
                        {
                            var rendered = false;

                            for ( var i = 0; i < links.Count; i++ )
                            {
                                var link = links[i];

                                if ( link.Type != "text/html" )
                                {
                                    continue;
                                }

                                if ( !rendered )
                                {
                                    description.Append( "<br/><br/>" );
                                    description.Append( "**Links**" );
                                    description.AppendLine( "<br/>" );
                                    rendered = true;
                                }

                                if ( StringSegment.IsNullOrEmpty( link.Title ) )
                                {
                                    if ( link.LinkTarget.IsAbsoluteUri )
                                    {
                                        description.AppendLine( $"- {link.LinkTarget.OriginalString}" );
                                    }
                                    else
                                    {
                                        description.AppendFormat( "- <a href=\"{0}\">{0}</a>", link.LinkTarget.OriginalString );
                                        description.AppendLine();
                                    }
                                }
                                else
                                {
                                    description.AppendLine( $"- [{link.Title}]({link.LinkTarget.OriginalString})" );
                                }
                            }
                        }

                        description.AppendLine().AppendLine( "<br/>" );
                        description.AppendLine( "**Additional Information**<br/>" );
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