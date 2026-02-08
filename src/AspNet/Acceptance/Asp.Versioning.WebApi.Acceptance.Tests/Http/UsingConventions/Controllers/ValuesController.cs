// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingConventions.Controllers;

using System.Web.Http;

[RoutePrefix( "api/values" )]
public class ValuesController : ApiController
{
    [Route]
    public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.RequestedApiVersion.ToString() } );

    [Route( "{id:int}" )]
    public IHttpActionResult Get( int id ) => Ok( new { controller = GetType().Name, id, version = Request.RequestedApiVersion.ToString() } );
}