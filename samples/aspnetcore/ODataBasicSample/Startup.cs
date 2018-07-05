namespace Microsoft.Examples
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Examples.Configuration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

    public class Startup
    {
        public Startup( IHostingEnvironment env )
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath( env.ContentRootPath )
                .AddJsonFile( "appsettings.json", optional: true, reloadOnChange: true )
                .AddJsonFile( $"appsettings.{env.EnvironmentName}.json", optional: true )
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices( IServiceCollection services )
        {
            services.AddMvc();

            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            services.AddApiVersioning( o => o.ReportApiVersions = true );
            services.AddOData().EnableApiVersioning();

            services.TryAddEnumerable( Transient<IModelConfiguration, OrderModelConfiguration>() );
            services.TryAddEnumerable( Transient<IModelConfiguration, PersonModelConfiguration>() );
        }

        public void Configure( IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, VersionedODataModelBuilder modelBuilder )
        {
            loggerFactory.AddConsole( Configuration.GetSection( "Logging" ) );
            loggerFactory.AddDebug();
            app.UseMvc( routeBuilder => routeBuilder.MapVersionedODataRoutes( "odata", "api", modelBuilder.GetEdmModels() ) );
        }
    }
}
