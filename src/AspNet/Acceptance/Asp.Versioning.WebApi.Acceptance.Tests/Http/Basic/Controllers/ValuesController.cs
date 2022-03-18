// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.Basic.Controllers;

using System.Web.Http;

[ApiVersion( "1.0" )]
[Route( "api/values" )]
public class ValuesController : ApiController
{
    public IHttpActionResult Get() => Ok( new { controller = GetType().Name, version = Request.GetRequestedApiVersion().ToString() } );
}