namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;

    public sealed class TestController : IHttpController
    {
        public Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken )
        {
            throw new NotImplementedException();
        }

        public Task<string> Get() => Task.FromResult( "Test" );
    }
}
