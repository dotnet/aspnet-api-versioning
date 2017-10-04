namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;

    [ControllerName( "Test" )]
    [ApiVersion( "2.0" )]
    [ApiVersion( "3.0" )]
    [ApiVersion( "1.8", Deprecated = true )]
    [ApiVersion( "1.9", Deprecated = true )]
    public sealed class TestVersion2Controller : IHttpController
    {
        public Task<HttpResponseMessage> ExecuteAsync( HttpControllerContext controllerContext, CancellationToken cancellationToken ) => throw new NotImplementedException();

        public Task<string> Get() => Task.FromResult( "Test" );

        [MapToApiVersion( "3.0" )]
        public Task<string> Get3() => Task.FromResult( "Test" );
    }
}