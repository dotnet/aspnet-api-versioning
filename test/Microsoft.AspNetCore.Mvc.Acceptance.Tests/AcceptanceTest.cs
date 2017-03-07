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
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading.Tasks;
    using TestHost;
    using Versioning;
    using Xunit;
    using static Microsoft.Extensions.DependencyInjection.ServiceDescriptor;
    using static System.Net.Http.HttpMethod;

    [Trait( "Kind", "Acceptance" )]
    [Trait( "Framework", "ASP.NET Core" )]
    public abstract class AcceptanceTest : IDisposable
    {
        const string JsonMediaType = "application/json";
        static readonly HttpMethod Patch = new HttpMethod( "PATCH" );
        readonly Lazy<TestServer> server;
        readonly Lazy<HttpClient> client;
        readonly FilteredControllerFeatureProvider filteredControllerTypes = new FilteredControllerFeatureProvider();
        bool disposed;

        ~AcceptanceTest()
        {
            Dispose( false );
        }

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

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
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

        protected virtual void OnConfigureRoutes( IRouteBuilder routeBuilder )
        {
        }

        HttpRequestMessage CreateRequest<TEntity>( string requestUri, TEntity entity, HttpMethod method )
        {
            var request = new HttpRequestMessage( method, requestUri );

            if ( !Equals( entity, default( TEntity ) ) )
            {
                var formatter = new JsonMediaTypeFormatter();
                request.Content = new ObjectContent<TEntity>( entity, formatter, JsonMediaType );
            }

            Client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( JsonMediaType ) );

            return request;
        }

        HttpRequestMessage CreateRequest( string requestUri, HttpContent content, HttpMethod method )
        {
            var request = new HttpRequestMessage( method, requestUri ) { Content = content };

            Client.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( JsonMediaType ) );

            return request;
        }

        protected virtual Task<HttpResponseMessage> GetAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Get ) );

        protected virtual Task<HttpResponseMessage> PostAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Post ) );

        protected virtual Task<HttpResponseMessage> PostAsync( string requestUri, HttpContent content ) => Client.SendAsync( CreateRequest( requestUri, content, Post ) );

        protected virtual Task<HttpResponseMessage> PutAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Put ) );

        protected virtual Task<HttpResponseMessage> PutAsync( string requestUri, HttpContent content ) => Client.SendAsync( CreateRequest( requestUri, content, Put ) );

        protected virtual Task<HttpResponseMessage> PatchAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Patch ) );

        protected virtual Task<HttpResponseMessage> PatchAsync( string requestUri, HttpContent content ) => Client.SendAsync( CreateRequest( requestUri, content, Patch ) );

        protected virtual Task<HttpResponseMessage> DeleteAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Delete ) );
    }
}