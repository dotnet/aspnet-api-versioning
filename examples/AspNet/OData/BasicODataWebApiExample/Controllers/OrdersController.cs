namespace ApiVersioning.Examples.Controllers;

using ApiVersioning.Examples.Models;
using Asp.Versioning;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using System.Web.Http;

[ApiVersion( 1.0 )]
[ODataRoutePrefix( "Orders" )]
public class OrdersController : ODataController
{
    // GET ~/api/v1/orders
    [ODataRoute]
    public IHttpActionResult Get( ODataQueryOptions<Order> options ) =>
        Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

    // GET ~/api/v1/orders/{id}
    [ODataRoute( "{id}" )]
    public IHttpActionResult Get( int id, ODataQueryOptions<Order> options ) =>
        Ok( new Order() { Id = id, Customer = "Bill Mei" } );
}