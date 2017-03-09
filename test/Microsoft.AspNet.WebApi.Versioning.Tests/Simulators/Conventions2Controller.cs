namespace Microsoft.Web.Http.Simulators
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Controllers;

    [ControllerName( "Conventions" )]
    [RoutePrefix( "api/conventions" )]
    public sealed class Conventions2Controller : ApiController
    {
        [Route]
        public Task<IHttpActionResult> Get() => Task.FromResult<IHttpActionResult>( Ok( $"Test ({Request.GetRequestedApiVersion()})" ) );

        [Route( "{id:int}" )]
        public Task<IHttpActionResult> Get( int id ) => Task.FromResult<IHttpActionResult>( Ok( $"Test {id} ({Request.GetRequestedApiVersion()})" ) );
    }
}