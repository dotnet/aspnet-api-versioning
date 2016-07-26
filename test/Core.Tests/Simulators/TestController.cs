namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    public sealed class TestController : Controller
    {
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}