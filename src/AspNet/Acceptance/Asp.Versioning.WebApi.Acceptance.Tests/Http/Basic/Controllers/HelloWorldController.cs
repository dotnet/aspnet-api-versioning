// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.Basic.Controllers;

using System.Web.Http;

[ApiVersion( "1.0" )]
[RoutePrefix( "api/v{version:apiVersion}/helloworld" )]
public class HelloWorldController : ApiController
{
    [Route]
    public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );

    [Route( "{id:int}", Name = "GetMessageById" )]
    public IHttpActionResult Get( int id ) => Ok( new { controller = GetType().Name, id, version = Request.GetRequestedApiVersion().ToString() } );

    [Route]
    public IHttpActionResult Post() => CreatedAtRoute( "GetMessageById", new { id = 42 }, default( object ) );

    [HttpGet]
    [Route( nameof( Unreachable ) )]
    [MapToApiVersion( "42.0" )]
    public IHttpActionResult Unreachable( ApiVersion apiVersion ) => Ok( new { Controller = GetType().Name, Version = apiVersion.ToString() } );
}