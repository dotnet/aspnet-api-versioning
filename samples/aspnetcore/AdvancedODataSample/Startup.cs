namespace Microsoft.Examples
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices( IServiceCollection services )
        {
            services.AddControllers();
            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;

                    // allows a client to make a request without specifying an api version. the value of
                    // options.DefaultApiVersion will be 'assumed'; this is meant to grandfather in legacy apis
                    options.AssumeDefaultVersionWhenUnspecified = true;

                    // allow multiple locations to request an api version
                    options.ApiVersionReader = ApiVersionReader.Combine(
                        new QueryStringApiVersionReader(),
                        new HeaderApiVersionReader( "api-version", "x-ms-version" ) );
                } );
            services.AddOData().EnableApiVersioning();
        }

        public void Configure( IApplicationBuilder app, VersionedODataModelBuilder modelBuilder )
        {
            app.UseRouting();
            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapVersionedODataRoute( "odata", "api", modelBuilder.GetEdmModels() );
                } );
        }
    }
}