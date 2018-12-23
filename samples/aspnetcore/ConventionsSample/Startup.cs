namespace Microsoft.Examples
{
    using Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
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
            // the sample application always uses the latest version, but you may want an explicit version such as Version_2_2
            // note: Endpoint Routing is enabled by default; however, if you need legacy style routing via IRouter, change it to false
            services.AddMvc( options => options.EnableEndpointRouting = true ).SetCompatibilityVersion( Latest );
            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;

                    options.Conventions.Controller<ValuesController>().HasApiVersion( 1, 0 );

                    options.Conventions.Controller<Values2Controller>()
                                       .HasApiVersion( 2, 0 )
                                       .HasApiVersion( 3, 0 )
                                       .Action( c => c.GetV3( default ) ).MapToApiVersion( 3, 0 )
                                       .Action( c => c.GetV3( default, default ) ).MapToApiVersion( 3, 0 );

                    options.Conventions.Controller<HelloWorldController>()
                                       .HasApiVersion( 1, 0 )
                                       .HasApiVersion( 2, 0 )
                                       .AdvertisesApiVersion( 3, 0 );
                } );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IHostingEnvironment env )
        {
            app.UseMvc();
        }
    }
}