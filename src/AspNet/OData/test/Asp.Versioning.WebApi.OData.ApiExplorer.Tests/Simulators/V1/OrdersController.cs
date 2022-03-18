// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.V1;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNet.OData;
using System.Web.Http;
using System.Web.Http.Description;

public class OrdersController : ODataController
{
    [ResponseType( typeof( ODataValue<Order> ) )]
    public IHttpActionResult Get( int key ) => Ok( new Order() { Id = key } );

    [ResponseType( typeof( ODataValue<Order> ) )]
    public IHttpActionResult Post( [FromBody] Order order )
    {
        order.Id = 42;
        return Created( order );
    }
}