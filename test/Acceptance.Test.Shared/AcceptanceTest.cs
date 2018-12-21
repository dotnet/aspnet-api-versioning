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
#if WEBAPI
    [Trait( "Framework", "Web API" )]
#else
    [Trait( "Framework", "ASP.NET Core" )]
#endif
    public abstract partial class AcceptanceTest
    {
        const string JsonMediaType = "application/json";
        static readonly HttpMethod Patch = new HttpMethod( "PATCH" );
        readonly HttpServerFixture fixture;

        protected AcceptanceTest( HttpServerFixture fixture ) => this.fixture = fixture;

        protected HttpClient Client => fixture.Client;

        HttpRequestMessage CreateRequest<TEntity>( string requestUri, TEntity entity, HttpMethod method )
        {
            AddDefaultAcceptHeaderIfNecessary();

            var request = new HttpRequestMessage( method, requestUri );

            if ( !Equals( entity, default( TEntity ) ) )
            {
                var formatter = new JsonMediaTypeFormatter();
                request.Content = new ObjectContent<TEntity>( entity, formatter, JsonMediaType );
            }

            return request;
        }

        HttpRequestMessage CreateRequest( string requestUri, HttpContent content, HttpMethod method )
        {
            AddDefaultAcceptHeaderIfNecessary();
            return new HttpRequestMessage( method, requestUri ) { Content = content };
        }

        protected virtual Task<HttpResponseMessage> GetAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Get ) );

        protected virtual Task<HttpResponseMessage> PostAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Post ) );

        protected virtual Task<HttpResponseMessage> PostAsync( string requestUri, HttpContent content ) => Client.SendAsync( CreateRequest( requestUri, content, Post ) );

        protected virtual Task<HttpResponseMessage> PutAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Put ) );

        protected virtual Task<HttpResponseMessage> PutAsync( string requestUri, HttpContent content ) => Client.SendAsync( CreateRequest( requestUri, content, Put ) );

        protected virtual Task<HttpResponseMessage> PatchAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Patch ) );

        protected virtual Task<HttpResponseMessage> PatchAsync( string requestUri, HttpContent content ) => Client.SendAsync( CreateRequest( requestUri, content, Patch ) );

        protected virtual Task<HttpResponseMessage> DeleteAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Delete ) );

        void AddDefaultAcceptHeaderIfNecessary()
        {
            var accept = Client.DefaultRequestHeaders.Accept;
            var comparer = StringComparer.OrdinalIgnoreCase;

            foreach ( var item in accept )
            {
                if ( comparer.Equals( item.MediaType, JsonMediaType ) )
                {
                    return;
                }
            }

            accept.Add( new MediaTypeWithQualityHeaderValue( JsonMediaType ) );
        }
    }
}