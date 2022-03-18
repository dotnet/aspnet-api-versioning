[assembly: Microsoft.AspNetCore.Mvc.ApiController]

namespace ApiVersioning.Examples;

using Asp.Versioning.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public Startup( IConfiguration configuration ) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    public void ConfigureServices( IServiceCollection services )
    {
        services.AddControllers();
        services.AddApiVersioning(
                    options =>
                    {
                        // reporting api versions will return the headers
                        // "api-supported-versions" and "api-deprecated-versions"
                        options.ReportApiVersions = true;
                    } )
                .AddMvc(
                    options =>
                    {
                        // automatically applies an api version based on the name of
                        // the defining controller's namespace
                        options.Conventions.Add( new VersionByNamespaceConvention() );
                    } );
    }

    public void Configure( IApplicationBuilder app )
    {
        app.UseRouting();
        app.UseEndpoints( builder => builder.MapControllers() );
    }
}