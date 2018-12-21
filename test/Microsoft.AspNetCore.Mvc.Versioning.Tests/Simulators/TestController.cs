namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    public sealed class TestController : ControllerBase
    {
        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}