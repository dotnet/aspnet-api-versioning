namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [ApiVersion( "5.0" )]
    [ApiVersion( "4.0", Deprecated = true )]
    [Route( "api/v{version:apiVersion}/attributed" )]
    public sealed class ApiVersionedRoute2Controller : ControllerBase
    {
        [HttpGet]
        [MapToApiVersion( "4.0" )]
        public Task<string> GetV4() => Task.FromResult( "Test" );

        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}