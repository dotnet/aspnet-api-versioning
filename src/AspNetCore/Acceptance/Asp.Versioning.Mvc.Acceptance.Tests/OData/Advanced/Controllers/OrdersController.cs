// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.OData.Advanced.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route( "api/orders" )]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public IActionResult Get( ApiVersion apiVersion ) =>
        Ok( new[] { new Order() { Id = 1, Customer = $"Customer v{apiVersion}" } } );

    [HttpGet( "{key}" )]
    public IActionResult Get( int key, ApiVersion apiVersion ) =>
        Ok( new Order() { Id = key, Customer = $"Customer v{apiVersion}" } );
}