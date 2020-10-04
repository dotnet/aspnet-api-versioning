namespace Microsoft.AspNetCore.OData.Basic.Controllers.Endpoint
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.OData.Models;

    [ApiVersion( "1.0" )]
    [Route( "api/[controller]" )]
    public class OrdersController : ODataController
    {
        [HttpGet]
        public IActionResult Get( ODataQueryOptions<Order> options ) =>
            Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

        [HttpGet( "{key}" )]
        public IActionResult Get( int key, ODataQueryOptions<Order> options ) =>
            Ok( new Order() { Id = key, Customer = "Bill Mei" } );
    }
}