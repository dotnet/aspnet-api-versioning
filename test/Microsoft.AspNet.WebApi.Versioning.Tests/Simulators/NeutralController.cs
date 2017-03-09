namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;

    [ApiVersionNeutral]
    public sealed class NeutralController : IHttpController
    {
        public Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken ) => throw new NotImplementedException();

        public Task<string> Get() => Task.FromResult( "Test" );
    }
}