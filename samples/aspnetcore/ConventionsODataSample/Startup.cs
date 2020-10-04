namespace Microsoft.Examples
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.Examples.Controllers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

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
            services.AddMvc( options => options.EnableEndpointRouting = false );
            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;

                    // apply api versions using conventions rather than attributes
                    options.Conventions.Controller<OrdersController>()
                                       .HasApiVersion( 1, 0 );

                    options.Conventions.Controller<PeopleController>()
                                       .HasApiVersion( 1, 0 )
                                       .HasApiVersion( 2, 0 )
                                       .Action( c => c.Patch( default, default, default ) ).MapToApiVersion( 2, 0 );

                    options.Conventions.Controller<People2Controller>()
                                       .HasApiVersion( 3, 0 );
                } );
            services.AddOData().EnableApiVersioning();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, VersionedODataModelBuilder modelBuilder )
        {
            app.UseMvc(
                routes =>
                {
                    // INFO: you do NOT and should NOT use both the query string and url segment methods together.
                    // this configuration is merely illustrating that they can coexist and allows you to easily
                    // experiment with either configuration. one of these would be removed in a real application.
                    //
                    // INFO: only pass the route prefix to GetEdmModels if you want to split the models; otherwise, both routes contain all models

                    // WHEN VERSIONING BY: query string, header, or media type
                    routes.MapVersionedODataRoute( "odata", "api", modelBuilder );

                    // WHEN VERSIONING BY: url segment
                    routes.MapVersionedODataRoute( "odata-bypath", "api/v{version:apiVersion}", modelBuilder );
                } );
        }
    }
}