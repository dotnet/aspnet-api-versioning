// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060

namespace Asp.Versioning.Simulators;

using Asp.Versioning.Models;
using System.Web.Http;
using static System.Net.HttpStatusCode;

[ControllerName( "Values" )]
public class Values3Controller : ApiController
{
    public IHttpActionResult Get() => Ok();

    public IHttpActionResult Get( int id ) => Ok();

    public IHttpActionResult Post( ClassWithId resource )
    {
        resource.Id = 2;
        return Created( "values/2", resource );
    }

    public IHttpActionResult Delete( int id ) => StatusCode( NoContent );
}