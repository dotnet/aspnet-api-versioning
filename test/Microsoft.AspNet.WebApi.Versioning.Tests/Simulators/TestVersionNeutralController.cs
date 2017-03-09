namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    [ApiVersionNeutral]
    [RoutePrefix( "api/neutral" )]
    public sealed class TestVersionNeutralController : IHttpController
    {
        public Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken ) => throw new NotImplementedException();

        [Route]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}