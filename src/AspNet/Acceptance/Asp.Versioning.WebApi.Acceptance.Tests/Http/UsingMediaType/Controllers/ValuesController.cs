// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingMediaType.Controllers;

using System.Web.Http;

[ApiVersion( "1.0" )]
[RoutePrefix( "api/values" )]
public class ValuesController : ApiController
{
    [Route]
    public IHttpActionResult Get() =>
        Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );

    [Route( "{id}" )]
    public IHttpActionResult Get( string id ) =>
        Ok( new { controller = GetType().Name, Id = id, version = Request.GetRequestedApiVersion().ToString() } );
}