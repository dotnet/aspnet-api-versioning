// Copyright (c) .NET Foundation and contributors. All rights reserved.

//// Ignore Spelling: Admin

namespace Asp.Versioning.Simulators;

using System.Web.Http;

[ApiVersionNeutral]
public class AdminController : ApiController
{
    [Route( "admin" )]
    public IHttpActionResult Get() => Ok();

    [HttpPost]
    public IHttpActionResult SeedData() => Ok();

    [HttpPost]
    public IHttpActionResult MarkAsTest() => Ok();

    [HttpPost]
    [Route( "admin/inject" )]
    public IHttpActionResult Inject() => Ok();
}