// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingMediaType.Controllers;

using Newtonsoft.Json.Linq;
using System.Web.Http;

[ApiVersion( "2.0" )]
[RoutePrefix( "api/values" )]
public class Values2Controller : ApiController
{
    [Route]
    public IHttpActionResult Get() =>
        Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );

    [Route( "{id}", Name = "GetByIdV2" )]
    public IHttpActionResult Get( string id ) =>
        Ok( new { controller = GetType().Name, Id = id, version = Request.GetRequestedApiVersion().ToString() } );

    public IHttpActionResult Post( [FromBody] JToken json ) =>
        CreatedAtRoute( "GetByIdV2", new { id = "42" }, json );
}