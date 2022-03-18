namespace ApiVersioning.Examples;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class Startup
{
    public Startup( IConfiguration configuration ) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    public void ConfigureServices( IServiceCollection services )
    {
        services.AddControllers().AddOData();
        services.AddApiVersioning()
                .AddOData(
                    options =>
                    {
                        // INFO: you do NOT and should NOT use both the query string and url segment methods together.
                        // this configuration is merely illustrating that they can coexist and allows you to easily
                        // experiment with either configuration. one of these would be removed in a real application.

                        // WHEN VERSIONING BY: query string, header, or media type
                        options.AddRouteComponents( "api" );

                        // WHEN VERSIONING BY: url segment
                        options.AddRouteComponents( "api/v{version:apiVersion}" );
                    } );
    }

    public void Configure( IApplicationBuilder app )
    {
        app.UseRouting();
        app.UseEndpoints( endpoints => endpoints.MapControllers() );
    }
}