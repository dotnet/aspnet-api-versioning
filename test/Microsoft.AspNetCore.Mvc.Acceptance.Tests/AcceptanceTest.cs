namespace Microsoft.AspNetCore.Mvc
{
    using ApplicationParts;
    using AspNetCore.Routing;
    using Builder;
    using Extensions.DependencyInjection;
    using Hosting;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using TestHost;
    using Versioning;
    using Xunit;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

    [Trait( "Framework", "ASP.NET Core" )]
    public abstract partial class AcceptanceTest : IDisposable
    {
        readonly Lazy<TestServer> server;
        readonly Lazy<HttpClient> client;

        protected AcceptanceTest()
        {
            server = new Lazy<TestServer>( CreateServer );
            client = new Lazy<HttpClient>( CreateAndInitializeHttpClient );
        }

        protected TestServer Server => server.Value;

        protected HttpClient Client => client.Value;

        protected ICollection<TypeInfo> FilteredControllerTypes => filteredControllerTypes;

        protected virtual void Dispose( bool disposing )
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

            if ( client.IsValueCreated )
            {
                client.Value.Dispose();
            }

            if ( server.IsValueCreated )
            {
                server.Value.Dispose();
            }
        }

        TestServer CreateServer()
        {
            var builder = new WebHostBuilder()
                .Configure( app => app.UseMvc( OnConfigureRoutes ).UseMvcWithDefaultRoute() )
                .ConfigureServices( OnDefaultConfigureServices )
                .UseContentRoot( GetContentRoot() );

            return new TestServer( builder );
        }

        HttpClient CreateAndInitializeHttpClient()
        {
            var newClient = Server.CreateClient();
            newClient.BaseAddress = new Uri( "http://localhost" );
            return newClient;
        }

        void OnDefaultConfigureServices( IServiceCollection services )
        {
            var partManager = new ApplicationPartManager();

            OnConfigurePartManager( partManager );
            services.Add( Singleton( partManager ) );
            services.AddMvc();
            services.AddApiVersioning( OnAddApiVersioning );
            OnConfigureServices( services );
        }

        string GetContentRoot()
        {
            var startupAssembly = GetType().GetTypeInfo().Assembly.GetName().Name;
            var contentRoot = new DirectoryInfo( AppContext.BaseDirectory );

            while ( contentRoot.Name != startupAssembly )
            {
                contentRoot = contentRoot.Parent;
            }

            return contentRoot.FullName;
        }

        protected virtual void OnConfigurePartManager( ApplicationPartManager partManager ) =>
            partManager.ApplicationParts.Add( new TestApplicationPart( FilteredControllerTypes ) );

        protected virtual void OnConfigureServices( IServiceCollection services ) { }

        protected abstract void OnAddApiVersioning( ApiVersioningOptions options );

        protected virtual void OnConfigureRoutes( IRouteBuilder routeBuilder ) { }
    }
}