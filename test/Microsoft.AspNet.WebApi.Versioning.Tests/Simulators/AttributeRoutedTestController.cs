namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    [RoutePrefix( "api/test" )]
    public sealed class AttributeRoutedTestController : ApiController
    {
        [Route]
        public Task<string> Get() => Task.FromResult( "Test" );

        [Route( "{id}" )]
        public Task<string> Get( string id ) => Task.FromResult( "Test" );
    }
}
