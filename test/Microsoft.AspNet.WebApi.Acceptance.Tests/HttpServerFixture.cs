namespace Microsoft.Web
{
    using System;
    using System.Net.Http;
    using System.Web.Http;
    using System.Web.Http.Dispatcher;
    using System.Web.Http.Tracing;
    using static System.Web.Http.IncludeErrorDetailPolicy;

    public abstract partial class HttpServerFixture
    {
        protected HttpServerFixture()
        {
            Configuration.IncludeErrorDetailPolicy = Always;
            Configuration.Services.Replace( typeof( IHttpControllerTypeResolver ), FilteredControllerTypes );
            Configuration.Services.Replace( typeof( ITraceWriter ), new TraceWriter() );
            Server = new HttpServer( Configuration );
            Client = new HttpClient( new HttpSimulatorHandler( Server ) ) { BaseAddress = new Uri( "http://localhost" ) };
        }

        public HttpConfiguration Configuration { get; } = new HttpConfiguration();

        public HttpServer Server { get; }

        public HttpClient Client { get; }

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
    }
}