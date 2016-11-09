namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using System;
    using System.Threading.Tasks;

    [AdvertiseApiVersions( "1.0", "2.0", "3.0" )]
    [AdvertiseApiVersions( "3.0-Alpha", Deprecated = true )]
    [ApiVersion( "4.0" )]
    [Route( "api/attributed" )]
    public sealed class AttributeRoutedTest4Controller : Controller
    {
        [HttpGet]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}
