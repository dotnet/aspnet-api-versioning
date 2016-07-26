namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [ApiVersion( "5.0" )]
    [ApiVersion( "4.0", Deprecated = true )]
    [Route( "api/v{version:apiVersion}/attributed" )]
    public sealed class ApiVersionedRoute2Controller : Controller
    {
        [MapToApiVersion( "4.0" )]
        public Task<string> GetV4() => Task.FromResult( "Test" );
        
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}
