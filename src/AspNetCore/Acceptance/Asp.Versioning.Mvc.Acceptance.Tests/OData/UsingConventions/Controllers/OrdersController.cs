// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.UsingConventions.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

public class OrdersController : ODataController
{
    [EnableQuery]
    public IActionResult Get( ODataQueryOptions<Order> options ) =>
        Ok( new[] { new Order() { Id = 1, Customer = "Bill Mei" } } );

    [EnableQuery]
    public IActionResult Get( int key, ODataQueryOptions<Order> options ) =>
        Ok( new Order() { Id = key, Customer = "Bill Mei" } );
}