namespace Microsoft.AspNetCore.OData.Advanced.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    [ApiController]
    [Route( "api/orders" )]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{HttpContext.GetRequestedApiVersion()}" } } );

        [HttpGet( "{key}" )]
        public IActionResult Get( int key ) => Ok( new Order() { Id = key, Customer = $"Customer v{HttpContext.GetRequestedApiVersion()}" } );
    }
}