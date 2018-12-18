namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [ApiController]
    [AdvertiseApiVersions( "1.0" )]
    [ApiVersion( "2.0" )]
    [ApiVersion( "3.0" )]
    [Route( "api/attributed" )]
    public sealed class AttributeRoutedTest2Controller : ControllerBase
    {
        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );

        [HttpGet]
        [MapToApiVersion( "3.0" )]
        public Task<string> GetV3() => Task.FromResult( "Test" );
    }
}