namespace Microsoft.Web
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Tracing;
    using Xunit;
    using static System.String;
    using static System.Web.Http.IncludeErrorDetailPolicy;

    [Trait( "Framework", "Web API" )]
    public abstract partial class AcceptanceTest : IDisposable
    {
        protected AcceptanceTest()
        {
            Configuration.IncludeErrorDetailPolicy = Always;
            Configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), FilteredControllerTypes );
            Configuration.Services.Replace( typeof( ITraceWriter ), new TraceWriter() );
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
    }
}