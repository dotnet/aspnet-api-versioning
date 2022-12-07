// Copyright (c) .NET Foundation and contributors. All rights reserved.

#pragma warning disable IDE0060

namespace Asp.Versioning.Http.Basic.Controllers;

using System.Web.Http;

[ApiVersion( "1.0" )]
[RoutePrefix( "api/v{version:apiVersion}/values" )]
public class OverlappingRouteTemplateController : ApiController
{
    [Route( "{id:int}/{childId}" )]
    public IHttpActionResult Get( int id, string childId ) => Ok( new { id, childId } );

    [Route( "{id:int}/children" )]
    public IHttpActionResult Get( int id ) => Ok( new { id } );

    [HttpGet]
    [Route( "{id:int}/ambiguous" )]
    public IHttpActionResult Ambiguous( int id ) => Ok();

    [HttpGet]
    [Route( "{id:int}/ambiguous" )]
    public IHttpActionResult Ambiguous2( int id ) => Ok();
}