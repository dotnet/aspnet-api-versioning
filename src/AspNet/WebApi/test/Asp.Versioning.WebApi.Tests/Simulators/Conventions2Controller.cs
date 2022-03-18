// Copyright (c) .NET Foundation and contributors. All rights reserved.

namespace Asp.Versioning.Simulators;

using System.Web.Http;

[ControllerName( "Conventions" )]
[RoutePrefix( "api/conventions" )]
public sealed class Conventions2Controller : ApiController
{
    [Route]
    public Task<IHttpActionResult> Get() => Task.FromResult<IHttpActionResult>( Ok( $"Test ({Request.GetRequestedApiVersion()})" ) );

    [Route( "{id:int}" )]
    public Task<IHttpActionResult> Get( int id ) => Task.FromResult<IHttpActionResult>( Ok( $"Test {id} ({Request.GetRequestedApiVersion()})" ) );
}