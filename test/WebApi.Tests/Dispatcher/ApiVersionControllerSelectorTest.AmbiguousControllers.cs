namespace Microsoft.Web.Http.Dispatcher
{
    using System;
    using System.Threading.Tasks;
    using System.Web.Http;

    public partial class ApiVersionControllerSelectorTest
    {
        [ApiVersion( "1.0" )]
        [RoutePrefix( "api/test" )]
        private sealed class Ambiguous1Controller : ApiController
        {
            [Route]
            public Task<string> Get() => Task.FromResult( "Test" );
        }

        [ApiVersion( "1.0" )]
        [RoutePrefix( "api/test" )]
        private sealed class Ambiguous2Controller : ApiController
        {
            [Route]
            public Task<string> Get() => Task.FromResult( "Test" );
        }

        [ApiVersion( "1.0" )]
        [ControllerName( "Ambiguous" )]
        private sealed class Ambiguous3Controller : ApiController
        {
            [Route]
            public Task<string> Get() => Task.FromResult( "Test" );
        }

        [ApiVersion( "1.0" )]
        private sealed class AmbiguousController : ApiController
        {
            [Route]
            public Task<string> Get() => Task.FromResult( "Test" );
        }

        [ApiVersionNeutral]
        [ControllerName( "Ambiguous" )]
        [RoutePrefix( "api/test" )]
        private sealed class AmbiguousNeutralController : ApiController
        {
            [Route]
            public Task<string> Get() => Task.FromResult( "Test" );
        }
    }
}
