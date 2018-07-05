﻿[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using global::Owin;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.Examples.Configuration;
    using Newtonsoft.Json.Serialization;
    using Swashbuckle.Application;
    using System.IO;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Description;

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
            const string routePrefix = default( string );
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );

            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            configuration.AddApiVersioning( o => o.ReportApiVersions = true );

            // note: this is required to make the default swagger json settings match the odata conventions applied by EnableLowerCamelCase()
            configuration.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var modelBuilder = new VersionedODataModelBuilder( configuration )
            {
                ModelBuilderFactory = () => new ODataConventionModelBuilder().EnableLowerCamelCase(),
                ModelConfigurations =
                {
                    new AllConfigurations(),
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration(),
                }
            };
            var models = modelBuilder.GetEdmModels();

            // TODO: while you can use both, you should choose only ONE of the following; comment, uncomment, or remove as necessary

            // WHEN VERSIONING BY: query string, header, or media type
            configuration.MapVersionedODataRoutes( "odata", routePrefix, models );

            // WHEN VERSIONING BY: url segment
            //configuration.MapVersionedODataRoutes( "odata-bypath", "api/v{apiVersion}", models, ConfigureODataServices );

            // add the versioned IApiExplorer and capture the strongly-typed implementation (e.g. ODataApiExplorer vs IApiExplorer)
            // note: the specified format code will format the version as "'v'major[.minor][-status]"
            var apiExplorer = configuration.AddODataApiExplorer(
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
                                          var description = "A sample application with Swagger, Swashbuckle, OData, and API versioning.";

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

                               // add a custom operation filter which documents the implicit API version parameter
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