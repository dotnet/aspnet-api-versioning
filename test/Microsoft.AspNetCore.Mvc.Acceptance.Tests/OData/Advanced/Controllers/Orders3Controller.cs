namespace Microsoft.AspNetCore.OData.Advanced.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    [ApiController]
    [ApiVersion( "3.0" )]
    [Route( "api/orders" )]
    public class Orders3Controller : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{HttpContext.GetRequestedApiVersion()}" } } );

        [HttpGet( "{key}" )]
        public IActionResult Get( int key ) => Ok( new Order() { Id = key, Customer = $"Customer v{HttpContext.GetRequestedApiVersion()}" } );
    }
}