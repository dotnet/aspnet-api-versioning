namespace Microsoft.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using Xunit;
    using static System.Net.Http.HttpMethod;
    using static System.String;
    using static System.Web.Http.IncludeErrorDetailPolicy;

    [Trait( "Kind", "Acceptance" )]
    [Trait( "Framework", "Web API" )]
    public abstract class AcceptanceTest : IDisposable
    {
        private sealed class FilteredControllerTypeResolver : List<Type>, IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes( IAssembliesResolver assembliesResolver ) => this;
        }

        private const string JsonMediaType = "application/json";
        private static readonly HttpMethod Patch = new HttpMethod( "PATCH" );
        private readonly FilteredControllerTypeResolver filteredControllerTypes = new FilteredControllerTypeResolver();
        private bool disposed;

        ~AcceptanceTest()
        {
            Dispose( false );
        }

        protected AcceptanceTest()
        {
            Configuration.IncludeErrorDetailPolicy = Always;
            Configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), FilteredControllerTypes );
            Server = new HttpServer( Configuration );
            Client = new HttpClient( new HttpSimulatorHandler( Server ) )
            {
                BaseAddress = new Uri( "http://localhost" ),
                DefaultRequestHeaders =
                {
                    { "Host", "localhost" }
                }
            };
        }

        protected HttpConfiguration Configuration { get; } = new HttpConfiguration();

        protected HttpServer Server { get; }

        protected HttpClient Client { get; }

        protected IList<Type> FilteredControllerTypes => filteredControllerTypes;

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

            Client.Dispose();
            Server.Dispose();
            Configuration.Dispose();
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        private HttpRequestMessage CreateRequest<TEntity>( string requestUri, TEntity entity, HttpMethod method )
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

        protected void Accept( string metadata = null )
        {
            var mediaType = new MediaTypeWithQualityHeaderValue( JsonMediaType );
            var odataMetadata = new NameValueHeaderValue( "odata.metadata" );

            if ( IsNullOrEmpty( metadata ) )
            {
                odataMetadata.Value = "none";
            }
            else
            {
                switch ( metadata.ToUpperInvariant() )
                {
                    case "NONE":
                    case "MINIMAL":
                    case "FULL":
                        break;
                    default:
                        throw new ArgumentOutOfRangeException( nameof( metadata ), "The specified metadata value must be 'none', 'minimal', or 'full'." );
                }

                odataMetadata.Value = metadata;
            }

            mediaType.Parameters.Add( odataMetadata );
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add( mediaType );
        }

        protected void PreferNoReturn() => Client.DefaultRequestHeaders.Add( "Prefer", "return=representation" );

        protected virtual Task<HttpResponseMessage> GetAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Get ) );

        protected virtual Task<HttpResponseMessage> PostAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Post ) );

        protected virtual Task<HttpResponseMessage> PutAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Put ) );

        protected virtual Task<HttpResponseMessage> PatchAsync<TEntity>( string requestUri, TEntity entity ) => Client.SendAsync( CreateRequest( requestUri, entity, Patch ) );

        protected virtual Task<HttpResponseMessage> DeleteAsync( string requestUri ) => Client.SendAsync( CreateRequest( requestUri, default( object ), Delete ) );
    }
}