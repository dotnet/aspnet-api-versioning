namespace Microsoft.AspNetCore.Mvc.Versioning
{
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [ApiVersion( "1.0" )]
    [ControllerName( "AmbiguousToo" )]
    public sealed class AmbiguousToo2Controller : ControllerBase
    {
        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}