namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    [ApiVersion( "1.0" )]
    [ApiVersion( "2.0" )]
    [ApiVersion( "3.0" )]
    [Route( "api/v{version:apiVersion}/test" )]
    public sealed class ApiVersionedRouteController : ApiController
    {
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}