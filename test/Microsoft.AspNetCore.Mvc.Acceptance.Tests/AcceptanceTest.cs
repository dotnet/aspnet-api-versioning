namespace Microsoft.AspNetCore.Mvc
{
    using ApplicationParts;
    using AspNetCore.Routing;
    using Builder;
    using Extensions.DependencyInjection;
    using Hosting;
    using Microsoft.AspNetCore.Mvc.Razor;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using TestHost;
    using Versioning;
    using Xunit;
    using static Microsoft.CodeAnalysis.MetadataReference;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
    using static System.Reflection.Assembly;

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
                .ConfigureServices( OnConfigureServices )
                .UseContentRoot( GetContentRoot() );

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
            services.Configure<RazorViewEngineOptions>( options =>
            {
                options.CompilationCallback += context =>
                {
                    var assembly = GetType().GetTypeInfo().Assembly;
                    var assemblies = assembly.GetReferencedAssemblies().Select( x => CreateFromFile( Load( x ).Location ) ).ToList();

                    assemblies.Add( CreateFromFile( Load( new AssemblyName( "mscorlib" ) ).Location ) );
                    assemblies.Add( CreateFromFile( Load( new AssemblyName( "System.Private.Corelib" ) ).Location ) );
                    assemblies.Add( CreateFromFile( Load( new AssemblyName( "System.Dynamic.Runtime" ) ).Location ) );
                    assemblies.Add( CreateFromFile( Load( new AssemblyName( "Microsoft.AspNetCore.Razor" ) ).Location ) );
                    context.Compilation = context.Compilation.AddReferences( assemblies );
                };
            } );
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

        protected abstract void OnAddApiVersioning( ApiVersioningOptions options );

        protected virtual void OnConfigureRoutes( IRouteBuilder routeBuilder ) { }
    }
}