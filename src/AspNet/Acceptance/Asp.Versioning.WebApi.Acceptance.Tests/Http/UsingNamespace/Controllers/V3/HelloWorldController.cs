// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Http.UsingNamespace.Controllers.V3;

using System.Web.Http;

[Route( "api/HelloWorld" )]
[Route( "api/{version:apiVersion}/HelloWorld" )]
public class HelloWorldController : ApiController
{
    public IHttpActionResult Get() => Ok( "V3" );
}