﻿[assembly: Microsoft.Owin.OwinStartup( typeof( Microsoft.Examples.Startup ) )]

namespace Microsoft.Examples
{
    using global::Owin;
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.Examples.Configuration;
    using Microsoft.OData;
    using System;
    using System.Web.Http;

    public class Startup
    {
        public void Configuration( IAppBuilder appBuilder )
        {
            var configuration = new HttpConfiguration();
            var httpServer = new HttpServer( configuration );

            // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
            configuration.AddApiVersioning( options => options.ReportApiVersions = true );

            var modelBuilder = new VersionedODataModelBuilder( configuration )
            {
                ModelConfigurations =
                {
                    new PersonModelConfiguration(),
                    new OrderModelConfiguration()
                }
            };

            // INFO: you do NOT and should NOT use both the query string and url segment methods together.
            // this configuration is merely illustrating that they can coexist and allows you to easily
            // experiment with either configuration. one of these would be removed in a real application.

            // WHEN VERSIONING BY: query string, header, or media type
            configuration.MapVersionedODataRoute( "odata", "api", modelBuilder );

            // WHEN VERSIONING BY: url segment
            configuration.MapVersionedODataRoute( "odata-bypath", "api/v{apiVersion}", modelBuilder );

            appBuilder.UseWebApi( httpServer );
        }

        public static string ContentRootPath
        {
            get
            {
                var app = AppDomain.CurrentDomain;

                if ( string.IsNullOrEmpty( app.RelativeSearchPath ) )
                {
                    return app.BaseDirectory;
                }

                return app.RelativeSearchPath;
            }
        }
    }
}