// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators.V2;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNet.OData;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using static System.Linq.Enumerable;

public class OrdersController : ODataController
{
    [ResponseType( typeof( ODataValue<IEnumerable<Order>> ) )]
    public IHttpActionResult Get() => Ok( Empty<Order>() );

    [ResponseType( typeof( ODataValue<Order> ) )]
    public IHttpActionResult Get( int id ) => Ok( new Order() { Id = id } );

    [ResponseType( typeof( ODataValue<Order> ) )]
    public IHttpActionResult Post( [FromBody] Order order )
    {
        order.Id = 42;
        return Created( order );
    }
}