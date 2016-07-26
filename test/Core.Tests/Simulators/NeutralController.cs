namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [ApiVersionNeutral]
    public sealed class NeutralController : Controller
    {
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}
