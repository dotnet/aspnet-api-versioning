namespace Microsoft.AspNetCore.Mvc
{
    using ApplicationParts;
    using AspNetCore.Routing;
    using Builder;
    using Extensions.DependencyInjection;
    using Hosting;
    using System;
    using System.Collections.Generic;
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
                .Configure( app => app.UseMvc( OnConfigureRoutes ) )
                .ConfigureServices( OnConfigureServices );

            return new TestServer( builder );
        }

        HttpClient CreateAndInitializeHttpClient()
        {
            var newClient = Server.CreateClient();
            newClient.BaseAddress = new Uri( "http://localhost" );
            return newClient;
        }

        void OnConfigureServices( IServiceCollection services )
        {
            var partManager = new ApplicationPartManager();

            partManager.FeatureProviders.Add( filteredControllerTypes );
            partManager.ApplicationParts.Add( new AssemblyPart( GetType().GetTypeInfo().Assembly ) );
            services.Add( Singleton( partManager ) );
            services.AddMvc();
            services.AddApiVersioning( OnAddApiVersioning );
        }

        protected abstract void OnAddApiVersioning( ApiVersioningOptions options );

        protected virtual void OnConfigureRoutes( IRouteBuilder routeBuilder ) { }
    }
}