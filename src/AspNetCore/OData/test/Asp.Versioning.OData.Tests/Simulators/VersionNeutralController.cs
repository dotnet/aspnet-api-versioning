// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;

[ApiVersionNeutral]
[ControllerName( "NeutralTests" )]
public class VersionNeutralController : ODataController
{
    [EnableQuery]
    public IActionResult Get() =>
        Ok( new TestNeutralEntity[] { new() { Id = 1 }, new() { Id = 2 }, new() { Id = 3 } } );

    [EnableQuery]
    public IActionResult Get( int key ) => Ok( new TestNeutralEntity() { Id = key } );
}