namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [Route( "api/attributed" )]
    public sealed class AttributeRoutedTestController : Controller
    {
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}
