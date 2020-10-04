namespace Microsoft.AspNetCore.OData.Advanced.Controllers.Endpoint
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    [ApiController]
    [Route( "api/orders" )]
    public class OrdersController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get( ApiVersion apiVersion ) => Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{apiVersion}" } } );

        [HttpGet( "{key}" )]
        public IActionResult Get( int key, ApiVersion apiVersion ) => Ok( new Order() { Id = key, Customer = $"Customer v{apiVersion}" } );
    }
}