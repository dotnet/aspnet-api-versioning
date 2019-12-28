namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using Microsoft.AspNet.OData.Builder;
    using Microsoft.AspNet.OData.Extensions;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using System;
    using System.Net.Http;
    using static Extensions.DependencyInjection.ServiceDescriptor;

    public class WebServer : IDisposable
    {
        readonly TestServer server;
        bool disposed;

        ~WebServer() => Dispose( false );

        public WebServer( Action<ApiVersioningOptions> setupApiVersioning = null, Action<IRouteBuilder> setupRoutes = null )
        {
            setupApiVersioning ??= _ => { };
            setupRoutes ??= _ => { };

            var hostBuilder = new WebHostBuilder()
                .ConfigureServices(
                    services =>
                    {
                        services.AddMvc( options => options.EnableEndpointRouting = false );
                        services.AddApiVersioning( setupApiVersioning );
                        services.AddOData().EnableApiVersioning();
                        services.Replace( Singleton<IActionSelector, TestODataApiVersionActionSelector>() );
                    } )
                .Configure(
                    app =>
                    {
                        var modelBuilder = app.ApplicationServices.GetRequiredService<VersionedODataModelBuilder>();

                        app.UseMvc( setupRoutes );
                        app.UseMvc( routeBuilder => routeBuilder.MapVersionedODataRoutes( "odata", "api", modelBuilder.GetEdmModels() ) );
                    } );

            server = new TestServer( hostBuilder );
            Client = server.CreateClient();
            Client.BaseAddress = new Uri( "http://localhost" );
        }

        public HttpClient Client { get; }

        public IServiceProvider Services => server.Host.Services;

        void Dispose( bool disposing )
        {
            if ( disposed )
            {
                return;
            }

            disposed = true;

            if ( !disposing )
            {
                return;
            }

            Client.Dispose();
            server.Dispose();
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }
    }
}