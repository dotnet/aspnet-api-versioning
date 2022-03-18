// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060 // Remove unused parameter

namespace Asp.Versioning.OData.UsingConventions.Controllers;

using Asp.Versioning.OData.Models;
using Microsoft.AspNet.OData;
using System.Web.Http;
using static System.Net.HttpStatusCode;

public class CustomersController : ODataController
{
    public IHttpActionResult Get() => Ok();

    public IHttpActionResult Get( int key ) => Ok();

    public IHttpActionResult Post( Customer customer )
    {
        customer.Id = 42;
        return Created( customer );
    }

    public IHttpActionResult Put( int key, [FromBody] Customer customer ) => StatusCode( NoContent );

    public IHttpActionResult Delete( int key ) => StatusCode( NoContent );
}