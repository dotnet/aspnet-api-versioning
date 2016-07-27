namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using AspNetCore.Routing;
    using Builder;
    using Extensions.DependencyInjection;
    using Extensions.DependencyInjection.Extensions;
    using Hosting;
    using Infrastructure;
    using System;
    using System.Net.Http;
    using TestHost;
    using static Extensions.DependencyInjection.ServiceDescriptor;

    public class WebServer : IDisposable
    {
        private readonly TestServer server;
        private bool disposed;

        ~WebServer()
        {
            Dispose( false );
        }

        public WebServer( Action<ApiVersioningOptions> setupApiVersioning = null, Action<IRouteBuilder> setupRoutes = null )
        {
            if ( setupApiVersioning == null )
            {
                setupApiVersioning = _ => { };
            }

            if ( setupRoutes == null )
            {
                setupRoutes = _ => { };
            }

            var hostBuilder = new WebHostBuilder()
                .Configure( app => app.UseMvc( setupRoutes ) )
                .ConfigureServices(
                    services =>
                    {
                        services.AddMvc();
                        services.AddApiVersioning( setupApiVersioning );
                        services.Replace( Singleton<IActionSelector, TestApiVersionActionSelector>() );
                    } );

            server = new TestServer( hostBuilder );
            Client = server.CreateClient();
            Client.BaseAddress = new Uri( "http://localhost" );
        }

        public HttpClient Client { get; }

        public IServiceProvider Services => server.Host.Services;

        private void Dispose( bool disposing )
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
