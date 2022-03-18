namespace ApiVersioning.Examples.Controllers;

using ApiVersioning.Examples.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

public class OrdersController : ODataController
{
    // GET ~/v1/orders
    [EnableQuery]
    public IActionResult Get( ODataQueryOptions<Order> options ) =>
        Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

    // GET ~/api/v1/orders/{key}
    [EnableQuery]
    public IActionResult Get( int key, ODataQueryOptions<Order> options ) =>
        Ok( new Order() { Id = key, Customer = "Bill Mei" } );
}