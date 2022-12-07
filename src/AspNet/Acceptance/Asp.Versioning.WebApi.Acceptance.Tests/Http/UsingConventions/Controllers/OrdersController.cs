// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060

namespace Asp.Versioning.Http.UsingConventions.Controllers;

using Asp.Versioning.Http.UsingConventions.Models;
using System.Web.Http;
using static System.Net.HttpStatusCode;

[RoutePrefix( "api/orders" )]
public class OrdersController : ApiController
{
    [Route]
    public IHttpActionResult Get() => Ok();

    [Route( "{id}", Name = "GetOrderById" )]
    public IHttpActionResult Get( int id ) => Ok();

    [Route]
    public IHttpActionResult Post( [FromBody] Order order )
    {
        order.Id = 42;
        return CreatedAtRoute( "GetOrderById", new { id = order.Id }, order );
    }

    [Route( "{id}" )]
    public IHttpActionResult Put( int id, [FromBody] Order order ) => StatusCode( NoContent );

    [Route( "{id}" )]
    public IHttpActionResult Delete( int id ) => StatusCode( NoContent );
}