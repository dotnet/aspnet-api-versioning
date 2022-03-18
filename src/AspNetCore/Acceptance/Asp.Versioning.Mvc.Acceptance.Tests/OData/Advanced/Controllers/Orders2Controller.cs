// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.Advanced.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

[ApiVersion( 2.0 )]
[ControllerName( "Orders" )]
public class Orders2Controller : ODataController
{
    [EnableQuery]
    public IActionResult Get( ODataQueryOptions<Order> options, ApiVersion apiVersion ) =>
        Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{apiVersion}" } } );

    [EnableQuery]
    public IActionResult Get( int key, ODataQueryOptions<Order> options, ApiVersion apiVersion ) =>
        Ok( new Order() { Id = key, Customer = $"Customer v{apiVersion}" } );
}