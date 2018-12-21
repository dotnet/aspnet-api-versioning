namespace Microsoft.Examples
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
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
            // the sample application always uses the latest version, but you may want an explict version such as Version_2_2
            // note: Endpoint Routing is enabled by default; however, if you need legacy style routing via IRouter, change it to false
            services.AddMvc( options => options.EnableEndpointRouting = true ).SetCompatibilityVersion( Latest );
            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;

                    // automatically applies an api version based on the name of the defining controller's namespace
                    options.Conventions.Add( new VersionByNamespaceConvention() );
                } );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IHostingEnvironment env )
        {
            app.UseMvc();
        }
    }
}