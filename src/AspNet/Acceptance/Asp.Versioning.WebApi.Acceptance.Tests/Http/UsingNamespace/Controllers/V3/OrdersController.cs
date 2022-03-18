// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingNamespace.Controllers.V3;

using Asp.Versioning.Http.UsingNamespace.Models;
using System.Web.Http;
using static System.Net.HttpStatusCode;

[RoutePrefix( "api/orders" )]
public class OrdersController : V2.OrdersController
{
    [Route]
    public override IHttpActionResult Get() => Ok();

    [Route( "{id}", Name = "GetOrderByIdV3" )]
    public override IHttpActionResult Get( int id ) => Ok();

    [Route]
    public override IHttpActionResult Post( [FromBody] Order order )
    {
        order.Id = 42;
        return CreatedAtRoute( "GetOrderByIdV3", new { id = order.Id }, order );
    }

    [Route( "{id}" )]
    public override IHttpActionResult Put( int id, [FromBody] Order order ) => StatusCode( NoContent );
}