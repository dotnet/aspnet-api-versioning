// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.Simulators.V3;

using Asp.Versioning.OData;
using Asp.Versioning.Simulators.Models;
using Microsoft.AspNet.OData;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Description;
using static System.Linq.Enumerable;
using static System.Net.HttpStatusCode;

public class OrdersController : ODataController
{
    [ResponseType( typeof( ODataValue<IEnumerable<Order>> ) )]
    public IHttpActionResult Get() => Ok( Empty<Person>() );

    [ResponseType( typeof( ODataValue<Order> ) )]
    public IHttpActionResult Get( int key ) => Ok( new Order() { Id = key } );

    [ResponseType( typeof( ODataValue<Order> ) )]
    public IHttpActionResult Post( [FromBody] Order order )
    {
        order.Id = 42;
        return Created( order );
    }

    public IHttpActionResult Delete( int key ) => StatusCode( NoContent );
}