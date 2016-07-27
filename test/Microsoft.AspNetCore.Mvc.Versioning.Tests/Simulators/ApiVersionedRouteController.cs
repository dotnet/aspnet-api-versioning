namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [ApiVersion( "1.0" )]
    [ApiVersion( "2.0" )]
    [ApiVersion( "3.0" )]
    [Route( "api/v{version:apiVersion}/attributed" )]
    public sealed class ApiVersionedRouteController : Controller
    {
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}
