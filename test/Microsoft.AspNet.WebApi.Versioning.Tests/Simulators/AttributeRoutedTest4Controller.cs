namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    [AdvertiseApiVersions( "1.0", "2.0", "3.0" )]
    [AdvertiseApiVersions( "3.0-Alpha", Deprecated = true )]
    [ApiVersion( "4.0" )]
    [RoutePrefix( "api/test" )]
    public sealed class AttributeRoutedTest4Controller : ApiController
    {
        [Route]
        public Task<string> Get() => Task.FromResult( "Test" );
    }
}
