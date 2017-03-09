namespace Microsoft.Web
{
    using System.Diagnostics.Contracts;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    sealed class HttpSimulatorHandler : DelegatingHandler
    {
        internal HttpSimulatorHandler( HttpMessageHandler innerHandler ) : base( innerHandler ) { }

        protected override async Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            request = await SimulateRequestOverTheWireAsync( request, cancellationToken );
            var response = await base.SendAsync( request, cancellationToken );
            return await SimulateResponseOverTheWireAsync( response, cancellationToken );
        }

        async Task<HttpRequestMessage> SimulateRequestOverTheWireAsync( HttpRequestMessage request, CancellationToken cancellationToken )
        {
            Contract.Requires( request != null );
            Contract.Requires( Contract.Result<Task<HttpRequestMessage>>() != null );

            var stream = new MemoryStream();
            HttpContent content = new HttpMessageContent( request );

            await content.CopyToAsync( stream );
            await stream.FlushAsync( cancellationToken );

            stream.Position = 0L;
            content = new StreamContent( stream );
            SetMediaType( content, "request" );

            return await content.ReadAsHttpRequestMessageAsync( cancellationToken );
        }

        async Task<HttpResponseMessage> SimulateResponseOverTheWireAsync( HttpResponseMessage response, CancellationToken cancellationToken )
        {
            Contract.Requires( response != null );
            Contract.Requires( Contract.Result<Task<HttpResponseMessage>>() != null );

            var stream = new MemoryStream();
            HttpContent content = new HttpMessageContent( response );

            await content.CopyToAsync( stream );
            await stream.FlushAsync( cancellationToken );

            stream.Position = 0L;
            content = new StreamContent( stream );
            SetMediaType( content, "response" );

            return await content.ReadAsHttpResponseMessageAsync( cancellationToken );
        }

        static void SetMediaType( HttpContent content, string messageType )
        {
            Contract.Requires( content != null );
            Contract.Requires( !string.IsNullOrEmpty( messageType ) );

            content.Headers.ContentType = new MediaTypeHeaderValue( "application/http" )
            {
                Parameters =
                {
                    new NameValueHeaderValue( "msgtype", messageType )
                }
            };
        }
    }
}