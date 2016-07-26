namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [ApiVersion( "2.0" )]
    [ApiVersion( "3.0" )]
    [ApiVersion( "1.8", Deprecated = true )]
    [ApiVersion( "1.9", Deprecated = true )]
    [ControllerName( "Test" )]
    public sealed class TestVersion2Controller : Controller
    {
        public Task<string> Get() => Task.FromResult( "Test" );

        [MapToApiVersion( "3.0-Alpha" )]
        [MapToApiVersion( "3.0" )]
        public Task<string> Get3() => Task.FromResult( "Test" );
    }
}
