﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.Basic.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using System.Web.Http;
using static System.Net.HttpStatusCode;

[ODataRoutePrefix( "Customers" )]
public class CustomersController : ODataController
{
    [ODataRoute]
    [ApiVersion( "2.0" )]
    [ApiVersion( "3.0" )]
    public IHttpActionResult Get() => Ok();

    [ODataRoute( "({key})" )]
    [ApiVersion( "1.0" )]
    [ApiVersion( "2.0" )]
    [ApiVersion( "3.0" )]
    public IHttpActionResult Get( int key ) => Ok();

    [ODataRoute]
    [ApiVersion( "1.0" )]
    [ApiVersion( "2.0" )]
    [ApiVersion( "3.0" )]
    public IHttpActionResult Post( [FromBody] Customer customer )
    {
        customer.Id = 42;
        return Created( customer );
    }

    [ODataRoute( "{key}" )]
    [ApiVersion( "3.0" )]
    public IHttpActionResult Put( int key, [FromBody] Customer customer ) => StatusCode( NoContent );

    [ODataRoute( "{key}" )]
    [ApiVersionNeutral]
    public IHttpActionResult Delete( int key ) => StatusCode( NoContent );
}