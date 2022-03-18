// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.Basic.Controllers;

using System.Web.Http;
using static System.Net.HttpStatusCode;

[ApiVersionNeutral]
[RoutePrefix( "api/ping" )]
public class PingController : ApiController
{
    [Route]
    public IHttpActionResult Get() => StatusCode( NoContent );
}