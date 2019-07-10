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
        readonly TestServer server;
        bool disposed;

        ~WebServer() => Dispose( false );

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
                .ConfigureServices(
                    services =>
                    {
                        services.AddMvc( options => options.EnableEndpointRouting = false );
                        services.AddApiVersioning( setupApiVersioning );
                        services.Replace( Singleton<IActionSelector, TestApiVersionActionSelector>() );
                    } )
                .Configure( app => app.UseMvc( setupRoutes ) );

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