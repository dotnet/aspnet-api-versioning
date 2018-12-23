namespace Microsoft.Examples
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using static Microsoft.AspNetCore.Mvc.CompatibilityVersion;

    public class Startup
    {
        public Startup( IConfiguration configuration )
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            // the sample application always uses the latest version, but you may want an explicit version such as Version_2_2
            // note: Endpoint Routing is enabled by default; however, it is unsupported by OData and MUST be false
            services.AddMvc( options => options.EnableEndpointRouting = false ).SetCompatibilityVersion( Latest );
            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;
                } );
            services.AddOData().EnableApiVersioning();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IHostingEnvironment env, VersionedODataModelBuilder modelBuilder )
        {
            app.UseMvc(
                routeBuilder =>
                {
                    var models = modelBuilder.GetEdmModels();
                    routeBuilder.MapVersionedODataRoutes( "odata", "api", models );
                    routeBuilder.MapVersionedODataRoutes( "odata-bypath", "v{version:apiVersion}", models );
                } );
        }
    }
}