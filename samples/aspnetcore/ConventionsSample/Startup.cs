namespace Microsoft.Examples
{
    using Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
            services.AddApiVersioning(
                options =>
                {
                    // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                    options.ReportApiVersions = true;

                    // apply api versions using conventions rather than attributes
                    options.Conventions.Controller<ValuesController>().HasApiVersion( 1, 0 );
                    options.Conventions.Controller<Values2Controller>()
                                       .HasApiVersion( 2, 0 )
                                       .HasApiVersion( 3, 0 )
                                       .Action( c => c.GetV3() ).MapToApiVersion( 3, 0 )
                                       .Action( c => c.GetV3( default( int ) ) ).MapToApiVersion( 3, 0 );
                    options.Conventions.Controller<HelloWorldController>()
                                       .HasApiVersion( 1, 0 )
                                       .HasApiVersion( 2, 0 )
                                       .AdvertisesApiVersion( 3, 0 );
                } );
        }

        public void Configure( IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory )
        {
            loggerFactory.AddConsole( Configuration.GetSection( "Logging" ) );
            loggerFactory.AddDebug();
            app.UseMvc();
            app.UseApiVersioning();
        }
    }
}