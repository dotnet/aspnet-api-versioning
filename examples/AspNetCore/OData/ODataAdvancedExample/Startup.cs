namespace ApiVersioning.Examples;

using Asp.Versioning;
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
        services.AddApiVersioning(
                    options =>
                    {
                        // reporting api versions will return the headers
                        // "api-supported-versions" and "api-deprecated-versions"
                        options.ReportApiVersions = true;

                        // allows a client to make a request without specifying an
                        // api version. the value of options.DefaultApiVersion will
                        // be 'assumed'; this is meant to grandfather in legacy apis
                        options.AssumeDefaultVersionWhenUnspecified = true;

                        // allow multiple locations to request an api version
                        options.ApiVersionReader = ApiVersionReader.Combine(
                            new QueryStringApiVersionReader(),
                            new HeaderApiVersionReader( "api-version", "x-ms-version" ) );
                    } )
                .AddOData( options => options.AddRouteComponents( "api" ) );
    }

    public void Configure( IApplicationBuilder app )
    {
        app.UseRouting();
        app.UseEndpoints( endpoints => endpoints.MapControllers() );
    }
}