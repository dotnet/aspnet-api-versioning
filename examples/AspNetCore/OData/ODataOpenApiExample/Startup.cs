namespace ApiVersioning.Examples;

using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IO;
using System.Reflection;
using static Microsoft.AspNetCore.OData.Query.AllowedQueryOptions;

/// <summary>
/// Represents the startup process for the application.
/// </summary>
public class Startup
{
    /// <summary>
    /// Configures services for the application.
    /// </summary>
    /// <param name="services">The collection of services to configure the application with.</param>
    public void ConfigureServices( IServiceCollection services )
    {
        services.AddControllers()
                .AddOData(
                    options =>
                    {
                        options.Count().Select().OrderBy();
                        options.RouteOptions.EnableKeyInParenthesis = false;
                        options.RouteOptions.EnableNonParenthesisForEmptyParameterFunction = true;
                        options.RouteOptions.EnableQualifiedOperationCall = false;
                        options.RouteOptions.EnableUnqualifiedOperationCall = true;
                    } );
        services.AddApiVersioning(
                    options =>
                    {
                        // reporting api versions will return the headers
                        // "api-supported-versions" and "api-deprecated-versions"
                        options.ReportApiVersions = true;

                        options.Policies.Sunset( 0.9 )
                                        .Effective( DateTimeOffset.Now.AddDays( 60 ) )
                                        .Link( "policy.html" )
                                            .Title( "Versioning Policy" )
                                            .Type( "text/html" );
                    } )
                .AddOData( options => options.AddRouteComponents( "api" ) )
                .AddODataApiExplorer(
                    options =>
                    {
                        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                        // note: the specified format code will format the version as "'v'major[.minor][-status]"
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
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(
            options =>
            {
                // add a custom operation filter which sets default values
                options.OperationFilter<SwaggerDefaultValues>();

                // integrate xml comments
                options.IncludeXmlComments( XmlCommentsFilePath );
            } );
    }

    /// <summary>
    /// Configures the application using the provided builder, hosting environment, and logging factory.
    /// </summary>
    /// <param name="app">The current application builder.</param>
    /// <param name="provider">The API version descriptor provider used to enumerate defined API versions.</param>
    public void Configure( IApplicationBuilder app, IApiVersionDescriptionProvider provider )
    {
        app.UseRouting();
        app.UseEndpoints( endpoints => endpoints.MapControllers() );
        app.UseSwagger();
        app.UseSwaggerUI(
            options =>
            {
                // build a swagger endpoint for each discovered API version
                foreach ( var description in provider.ApiVersionDescriptions )
                {
                    options.SwaggerEndpoint( $"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant() );
                }
            } );
    }

    private static string XmlCommentsFilePath
    {
        get
        {
            var basePath = PlatformServices.Default.Application.ApplicationBasePath;
            var fileName = typeof( Startup ).GetTypeInfo().Assembly.GetName().Name + ".xml";
            return Path.Combine( basePath, fileName );
        }
    }
}