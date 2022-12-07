// Copyright (c) .NET Foundation and contributors. All rights reserved.


namespace Asp.Versioning.Simulators;

using System.Web.Http;

[ApiVersion( "2015-11-15" )]
[ApiVersion( "2016-06-06" )]
public class OrdersController : ApiController
{
    [MapToApiVersion( "2015-11-15" )]
    public Task<IHttpActionResult> Get_2015_11_15() => Task.FromResult<IHttpActionResult>( Ok( "Version 2015-11-15" ) );

    [Route( "orders" )]
    [MapToApiVersion( "2016-06-06" )]
    public Task<IHttpActionResult> Get_2016_06_06() => Task.FromResult<IHttpActionResult>( Ok( "Version 2016-06-06" ) );
}