namespace Microsoft.Examples.Controllers
{
    using Microsoft.AspNet.OData;
    using Microsoft.AspNet.OData.Query;
    using Microsoft.AspNetCore.Mvc;
    using Models;

    [ApiVersion( "1.0" )]
    [ApiVersion( "2.0" )]
    public class OrdersController : ODataController
    {
        // GET ~/v1/orders
        [HttpGet]
        public IActionResult Get( ODataQueryOptions<Order> options ) =>
            Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

        // GET ~/api/v1/orders/{key}?api-version=1.0
        [HttpGet( "{key:int}" )]
        public IActionResult Get( int key, ODataQueryOptions<Order> options ) =>
            Ok( new Order() { Id = key, Customer = "Bill Mei" } );
    }
}