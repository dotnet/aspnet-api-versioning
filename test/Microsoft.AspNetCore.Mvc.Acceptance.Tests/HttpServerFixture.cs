namespace Microsoft.AspNetCore.Mvc
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Versioning;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using static Microsoft.AspNetCore.Mvc.CompatibilityVersion;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;

    public abstract partial class HttpServerFixture
    {
        readonly Lazy<TestServer> server;
        readonly Lazy<HttpClient> client;

        protected HttpServerFixture()
        {
            server = new Lazy<TestServer>( CreateServer );
            client = new Lazy<HttpClient>( CreateAndInitializeHttpClient );
        }

        public TestServer Server => server.Value;

        public HttpClient Client => client.Value;

        public bool EnableEndpointRouting { get; protected set; }

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

        protected virtual void OnConfigurePartManager( ApplicationPartManager partManager ) =>
            partManager.ApplicationParts.Add( new TestApplicationPart( FilteredControllerTypes ) );

        protected virtual void OnConfigureServices( IServiceCollection services ) { }

        protected abstract void OnAddApiVersioning( ApiVersioningOptions options );

        protected virtual void OnConfigureRoutes( IRouteBuilder routeBuilder ) { }

#if !NET461
        protected virtual void OnConfigureEndpoints( IEndpointRouteBuilder routeBuilder )
        {
            routeBuilder.MapControllers();
            routeBuilder.MapDefaultControllerRoute();
        }
#endif

        TestServer CreateServer()
        {
            var builder = new WebHostBuilder()
                .ConfigureServices( OnDefaultConfigureServices )
                .Configure( OnConfigureApplication )
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
            services.AddMvc( options => options.EnableEndpointRouting = EnableEndpointRouting ).SetCompatibilityVersion( Latest );
            services.AddApiVersioning( OnAddApiVersioning );
            OnConfigureServices( services );
        }

        void OnConfigureApplication( IApplicationBuilder app )
        {
#if NET461
            app.UseMvc( OnConfigureRoutes ).UseMvcWithDefaultRoute();
#else
            if ( EnableEndpointRouting )
            {
                app.UseRouting();
                app.UseEndpoints( OnConfigureEndpoints );
            }
            else
            {
                app.UseMvc( OnConfigureRoutes ).UseMvcWithDefaultRoute();
            }
#endif
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
    }
}