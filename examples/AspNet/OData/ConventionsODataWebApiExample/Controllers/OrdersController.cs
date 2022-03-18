namespace ApiVersioning.Examples.Controllers;

using ApiVersioning.Examples.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using System.Web.Http;

public class OrdersController : ODataController
{
    // GET ~/api/v1/orders
    public IHttpActionResult Get( ODataQueryOptions<Order> options ) =>
        Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

    // GET ~/api/v1/orders/{key}
    public IHttpActionResult Get( int key, ODataQueryOptions<Order> options ) =>
        Ok( new Order() { Id = key, Customer = "Bill Mei" } );
}