#if WEBAPI
namespace Microsoft.Web
#else
namespace Microsoft.AspNetCore.Mvc
#endif
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Xunit;
    using static System.Net.Http.HttpMethod;

    [Trait( "Kind", "Acceptance" )]
    public abstract partial class AcceptanceTest : IDisposable
    {
        const string JsonMediaType = "application/json";
        static readonly HttpMethod Patch = new HttpMethod( "PATCH" );
        readonly FilteredControllerTypes filteredControllerTypes = new FilteredControllerTypes();
        bool disposed;

        ~AcceptanceTest() => Dispose( false );

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
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