// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.Basic.Controllers;

using System.Web.Http;

[ApiVersion( "2.0" )]
[Route( "api/values" )]
public class Values2Controller : ApiController
{
    public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );
}