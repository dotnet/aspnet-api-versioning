namespace Microsoft.AspNetCore.Mvc.Simulators
{
    using Routing;
    using System;
    using System.Threading.Tasks;

    [ApiVersion( "2015-11-15" )]
    [ApiVersion( "2016-06-06" )]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public Task<IActionResult> Get() => Task.FromResult<IActionResult>( Ok( "Version 2015-11-15" ) );

        [HttpGet]
        [MapToApiVersion( "2016-06-06" )]
        public Task<IActionResult> Get_2016_06_06() => Task.FromResult<IActionResult>( Ok( "Version 2016-06-06" ) );
    }
}