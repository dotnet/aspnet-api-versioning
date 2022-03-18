// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

[ApiVersion( 2.0 )]
[ControllerName( "Tests" )]
public class Tests2Controller : ODataController
{
    [EnableQuery]
    public IActionResult Get() =>
        Ok( new TestEntity[] { new() { Id = 1 }, new() { Id = 2 }, new() { Id = 3 } } );

    [EnableQuery]
    public IActionResult Get( int key ) => Ok( new TestEntity() { Id = key } );
}